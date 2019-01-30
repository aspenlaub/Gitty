using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using LibGit2Sharp;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class CakeInstaller : ICakeInstaller {
        private readonly IGitUtilities vGitUtilities;

        public CakeInstaller(IGitUtilities gitUtilities) {
            vGitUtilities = gitUtilities;
        }

        public void InstallCake(IFolder cakeFolder, out IErrorsAndInfos errorsAndInfos) {
            errorsAndInfos = new ErrorsAndInfos();
            const string url = "https://github.com/cake-build/example";
            var fixCakeErrorsAndInfos = new ErrorsAndInfos();
            vGitUtilities.Clone(url, cakeFolder, new CloneOptions { BranchName = "master" }, true, () => File.Exists(CakeExeFileFullName(cakeFolder)), () => FixCakeVersionAndDownloadReadyToCake(cakeFolder, fixCakeErrorsAndInfos), errorsAndInfos);
            if (fixCakeErrorsAndInfos.AnyErrors() && !errorsAndInfos.AnyErrors()) {
                errorsAndInfos = fixCakeErrorsAndInfos;
            }
        }

        private void FixCakeVersionAndDownloadReadyToCake(IFolder cakeFolder, IErrorsAndInfos errorsAndInfos) {
            var packagesConfigFileFullName = cakeFolder.SubFolder("tools").FullName + @"\packages.config";
            var document = XDocument.Load(packagesConfigFileFullName);
            var element = document.XPathSelectElements("/packages/package").FirstOrDefault(e => e.Attribute("id")?.Value == "Cake");
            if (element == null) {
                errorsAndInfos.Errors.Add("Could not find package element");
                return;
            }
            var attribute = element.Attribute("version");
            if (attribute == null) {
                errorsAndInfos.Errors.Add("Could not find version attribute");
                return;
            }
            attribute.SetValue(CakeRunner.PinnedCakeVersion);
            document.Save(packagesConfigFileFullName);

            vGitUtilities.DownloadReadyToCake(cakeFolder.SubFolder(@"tools"), errorsAndInfos);
        }

        public string CakeExeFileFullName(IFolder cakeFolder) {
            return cakeFolder.SubFolder("tools").SubFolder("Cake").FullName + @"\cake.exe";
        }
    }
}
