using System.Collections.Generic;
using System.IO;
using System.Net;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using ICSharpCode.SharpZipLib.Zip;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class CakeInstaller : ICakeInstaller {
        private readonly IGitUtilities vGitUtilities;

        public CakeInstaller(IGitUtilities gitUtilities) {
            vGitUtilities = gitUtilities;
        }

        public void InstallCake(IFolder toolsParentFolder, out IErrorsAndInfos errorsAndInfos) {
            errorsAndInfos = new ErrorsAndInfos();
            DownloadReadyToCake(toolsParentFolder.SubFolder(@"tools"), errorsAndInfos);
        }

        public string CakeExeFileFullName(IFolder toolsParentFolder) {
            return toolsParentFolder.SubFolder("tools").SubFolder("Cake").FullName + @"\cake.exe";
        }

        public void DownloadReadyToCake(IFolder toolsFolder, IErrorsAndInfos errorsAndInfos) {
            var downloadFolder = vGitUtilities.DownloadFolder();
            var downloadedZipFileFullName = downloadFolder + $"\\cake.{CakeRunner.PinnedCakeVersion}.zip";
            if (!File.Exists(downloadedZipFileFullName)) {
                var url = $"https://www.aspenlaub.net/Github/cake.{CakeRunner.PinnedCakeVersion}.zip";
                using (var client = new WebClient()) {
                    client.DownloadFile(url, downloadedZipFileFullName);
                }

                if (!File.Exists(downloadedZipFileFullName)) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotDownload, url));
                }
            }

            using (var zipStream = new FileStream(downloadedZipFileFullName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var fastZip = new FastZip();
                fastZip.ExtractZip(zipStream, toolsFolder.FullName, FastZip.Overwrite.Never, s => { return true; }, null, null, true, true);
                if (!toolsFolder.SubFolder("Cake").Exists()) {
                    errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FolderCouldNotBeCreated, toolsFolder.SubFolder("Cake").FullName));
                    return;
                }
            }

            var packagesConfigFileName = toolsFolder.FullName + @"\packages.config";
            if (File.Exists(packagesConfigFileName)) { return; }

            File.WriteAllLines(packagesConfigFileName, new List<string> {
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
                "<packages>",
                "<package id=\"Cake\" version=\"" + CakeRunner.PinnedCakeVersion + "\" />",
                "</packages>"
            });
        }

    }
}
