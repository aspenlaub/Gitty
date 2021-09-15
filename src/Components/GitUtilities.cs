using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components {
    public class GitUtilities : IGitUtilities {
        public string CheckedOutBranch(IFolder folder) {
            while (folder.Exists()) {
                if (!folder.GitSubFolder().Exists()) {
                    folder = folder.ParentFolder();
                    if (folder == null) { return ""; }

                    continue;
                }

                using var repo = new Repository(folder.FullName, new RepositoryOptions());
                return repo.Head.FriendlyName;
            }

            return "";
        }

        public void Clone(string url, string branch, IFolder folder, CloneOptions cloneOptions, bool useCache, IErrorsAndInfos errorsAndInfos) {
            Clone(url, branch, folder, cloneOptions, useCache, () => true, () => { }, errorsAndInfos);
        }

        public void Clone(string url, string branch, IFolder folder, CloneOptions cloneOptions, bool useCache, Func<bool> extraCacheCondition, Action onCloned, IErrorsAndInfos errorsAndInfos) {
            var canCloneBeUsed = useCache && CloneFromCache(url, branch, folder);
            var zipFileName = CloneZipFileName(url, branch);
            if (canCloneBeUsed && !extraCacheCondition()) {
                canCloneBeUsed = false;
                if (folder.Exists()) {
                    var deleter = new FolderDeleter();
                    deleter.DeleteFolder(folder);
                }
                File.Delete(zipFileName);
            }
            if (canCloneBeUsed) { return; }

            Repository.Clone(url, folder.FullName, cloneOptions);
            onCloned();
            if (!useCache) { return; }

            if (File.Exists(zipFileName)) { return; }

            DeleteFolderLinks(folder);

            var fastZip = new FastZip();
            fastZip.CreateZip(zipFileName, folder.FullName, true, "");
        }

        private static void DeleteFolderLinks(IFolder folder) {
            var directories = Directory.GetDirectories(folder.FullName, "_git*", SearchOption.TopDirectoryOnly)
                .Select(d => new DirectoryInfo(d))
                .Where(d => (d.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                .ToList();
            foreach (var directory in directories) {
                directory.Delete();
            }
        }

        protected bool CloneFromCache(string url, string branch, IFolder folder) {
            DeleteOldDownloadFiles("*---*.*");

            var zipFileName = CloneZipFileName(url, branch);
            if (!File.Exists(zipFileName)) { return false; }

            var fastZip = new FastZip();
            fastZip.ExtractZip(zipFileName, folder.FullName, FastZip.Overwrite.Always, _ => true, null, null, true);
            return true;
        }

        private string CloneZipFileName(string url, string branch) {
            return DownloadFolder() + '\\' + url.Replace(':', '-').Replace('/', '-').Replace('.', '-') + (branch == "master" ? "" : '-' + branch) + ".zip";
        }

        public string DownloadFolder() {
            var downloadFolder = Path.GetTempPath() + @"\AspenlaubDownloads";
            if (!Directory.Exists(downloadFolder)) {
                Directory.CreateDirectory(downloadFolder);
            }
            return downloadFolder;
        }

        public string HeadTipIdSha(IFolder repositoryFolder) {
            if (!repositoryFolder.Exists()) { return ""; }

            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            return repo.Head.Tip.Id.Sha;
        }

        public void VerifyThatThereAreNoUncommittedChanges(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            var files = FilesWithUncommittedChanges(repositoryFolder);
            foreach (var file in files) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.UncommittedChangeTo, file));
            }
        }

        public IList<string> FilesWithUncommittedChanges(IFolder repositoryFolder) {
            if (!repositoryFolder.Exists()) { return new List<string>(); }

            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            return repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory).Select(c => c.Path).ToList();
        }

        protected void DeleteOldDownloadFiles(string wildcard) {
            var downloadFolder = DownloadFolder();
            if (!Directory.Exists(downloadFolder)) { return; }

            foreach (var file in Directory.GetFiles(downloadFolder, wildcard).Where(f => File.GetLastWriteTime(f).AddDays(1) < DateTime.Now)) {
                File.Delete(file);
            }
        }

        public void Reset(IFolder repositoryFolder, string headTipIdSha, IErrorsAndInfos errorsAndInfos) {
            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            var commit = repo.Head.Commits.FirstOrDefault(c => c.Sha == headTipIdSha);
            if (commit == null) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CommitNotFound, headTipIdSha));
            } else {
                repo.Reset(ResetMode.Hard, commit);
                repo.RemoveUntrackedFiles();
            }
        }

        public bool IsBranchAheadOfMaster(IFolder repositoryFolder) {
            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            var head = repo.Head;
            var masterBranch = repo.Branches["origin/master"];
            var divergence = repo.ObjectDatabase.CalculateHistoryDivergence(head.Tip, masterBranch.Tip);
            return divergence.AheadBy > 0;
        }

        public void IdentifyOwnerAndName(IFolder repositoryFolder, out string owner, out string name, IErrorsAndInfos errorsAndInfos) {
            owner = "";
            name = "";

            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            var remotes = repo.Network.Remotes.ToList();
            if (remotes.Count != 1) {
                errorsAndInfos.Errors.Add(Properties.Resources.ExactlyOneRemoteExpected);
                return;
            }

            var url = remotes.First().Url;
            var urlComponents = url.Split('/');
            if ("github.com" != urlComponents[^3]) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CannotInterpretRepositoryUrl, url));
                return;
            }

            owner = urlComponents[^2];
            name = urlComponents[^1];
            if (!name.EndsWith(".git")) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CannotInterpretRepositoryUrl, url));
                return;
            }

            name = name.Substring(0, name.Length - 4);
        }

        public void Pull(IFolder repositoryFolder, string author, string eMail) {
            using var repo = new Repository(repositoryFolder.FullName);
            var options = new PullOptions();
            var signature = new Signature(new Identity(author, eMail), DateTimeOffset.Now);
            Commands.Pull(repo, signature, options);
        }

        public IList<string> AllIdShas(IFolder repositoryFolder) {
            using var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions());
            return repo.Head.Commits.Select(c => c.Sha).ToList();
        }
    }
}
