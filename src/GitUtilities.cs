using System;
using System.IO;
using System.Linq;
using System.Net;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class GitUtilities : IGitUtilities {
        public string CheckedOutBranch(IFolder folder) {
            while (folder.Exists()) {
                if (!folder.GitSubFolder().Exists()) {
                    folder = folder.ParentFolder();
                    if (folder == null) { return ""; }

                    continue;
                }

                using (var repo = new Repository(folder.FullName, new RepositoryOptions())) {
                    return repo.Head.FriendlyName;
                }
            }

            return "";
        }

        public void Clone(string url, IFolder folder, CloneOptions cloneOptions, bool useCache, IErrorsAndInfos errorsAndInfos) {
            Clone(url, folder, cloneOptions, useCache, () => true, () => { }, errorsAndInfos);
        }

        public void Clone(string url, IFolder folder, CloneOptions cloneOptions, bool useCache, Func<bool> extraCacheCondition, Action onCloned, IErrorsAndInfos errorsAndInfos) {
            var canCloneBeUsed = useCache && CloneFromCache(url, folder);
            var zipFileName = CloneZipFileName(url);
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

            var fastZip = new FastZip();
            fastZip.CreateZip(zipFileName, folder.FullName, true, "");
        }

        protected bool CloneFromCache(string url, IFolder folder) {
            DeleteOldDownloadFiles("*---*.*");

            var zipFileName = CloneZipFileName(url);
            if (!File.Exists(zipFileName)) { return false; }

            var fastZip = new FastZip();
            fastZip.ExtractZip(zipFileName, folder.FullName, FastZip.Overwrite.Always, s => true, null, null, true);
            return true;
        }

        private static string CloneZipFileName(string url) {
            return DownloadFolder() + '\\' + url.Replace(':', '-').Replace('/', '-').Replace('.', '-') + ".zip";
        }

        private static string DownloadFolder() {
            var downloadFolder = Path.GetTempPath() + @"\AspenlaubDownloads";
            if (!Directory.Exists(downloadFolder)) {
                Directory.CreateDirectory(downloadFolder);
            }
            return downloadFolder;
        }

        public string HeadTipIdSha(IFolder repositoryFolder) {
            if (!repositoryFolder.Exists()) { return ""; }

            using (var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions())) {
                return repo.Head.Tip.Id.Sha;
            }
        }

        public void VerifyThatThereAreNoUncommittedChanges(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            if (!repositoryFolder.Exists()) { return; }

            using (var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions())) {
                var changes = repo.Diff.Compare<TreeChanges>(repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory).ToList();
                foreach (var change in changes) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.UncommittedChangeTo, change.Path));
                }
            }
        }

        protected static void DeleteOldDownloadFiles(string wildcard) {
            var downloadFolder = DownloadFolder();
            if (!Directory.Exists(downloadFolder)) { return; }

            foreach (var file in Directory.GetFiles(downloadFolder, wildcard).Where(f => File.GetLastWriteTime(f).AddMinutes(30) < DateTime.Now)) {
                File.Delete(file);
            }
        }

        public void Reset(IFolder repositoryFolder, string headTipIdSha, IErrorsAndInfos errorsAndInfos) {
            using (var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions())) {
                var commit = repo.Head.Commits.FirstOrDefault(c => c.Sha == headTipIdSha);
                if (commit == null) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CommitNotFound, headTipIdSha));
                } else {
                    repo.Reset(ResetMode.Hard, commit);
                }
            }
        }

        public bool IsBranchAheadOfMaster(IFolder repositoryFolder) {
            using (var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions())) {
                var head = repo.Head;
                var masterBranch = repo.Branches["origin/master"];
                var divergence = repo.ObjectDatabase.CalculateHistoryDivergence(head.Tip, masterBranch.Tip);
                return divergence.AheadBy > 0;

            }
        }

        public void IdentifyOwnerAndName(IFolder repositoryFolder, out string owner, out string name, IErrorsAndInfos errorsAndInfos) {
            owner = "";
            name = "";

            using (var repo = new Repository(repositoryFolder.FullName, new RepositoryOptions())) {
                var remotes = repo.Network.Remotes.ToList();
                if (remotes.Count != 1) {
                    errorsAndInfos.Errors.Add(Properties.Resources.ExactlyOneRemoteExpected);
                    return;
                }

                var url = remotes.First().Url;
                var urlComponents = url.Split('/');
                if ("github.com" != urlComponents[urlComponents.Length - 3]) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CannotInterpretRepositoryUrl, url));
                    return;
                }

                owner = urlComponents[urlComponents.Length - 2];
                name = urlComponents[urlComponents.Length - 1];
                if (!name.EndsWith(".git")) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CannotInterpretRepositoryUrl, url));
                    return;
                }

                name = name.Substring(0, name.Length - 4);
            }
        }

        public void DownloadReadyToCake(IFolder folder, IErrorsAndInfos errorsAndInfos) {
            DeleteOldDownloadFiles($"cake.{CakeRunner.PinnedCakeVersion}.zip");
            var downloadFolder = DownloadFolder();
            var downloadedZipFileFullName = downloadFolder + $"\\cake.{CakeRunner.PinnedCakeVersion}.zip";
            if (!File.Exists(downloadedZipFileFullName)) {
                using (var client = new WebClient()) {
                    client.DownloadFile($"https://www.aspenlaub.net/Github/cake.{CakeRunner.PinnedCakeVersion}.zip", downloadedZipFileFullName);
                }
            }
            using (var zipStream = new FileStream(downloadedZipFileFullName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var fastZip = new FastZip();
                fastZip.ExtractZip(zipStream, folder.FullName, FastZip.Overwrite.Never, s => { return true; }, null, null, true, true);
                if (Directory.Exists(folder.FullName + @"\Cake")) { return; }

                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FolderCouldNotBeCreated, folder));
            }
        }
    }
}
