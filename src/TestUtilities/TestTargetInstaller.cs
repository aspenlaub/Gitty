using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class TestTargetInstaller {
        private readonly ICakeInstaller vCakeInstaller;

        public TestTargetInstaller(ICakeInstaller cakeInstaller) {
            vCakeInstaller = cakeInstaller;
        }

        public void DeleteCakeFolder(ITestTargetFolder testTargetFolder) {
            if (!testTargetFolder.CakeFolder().Exists()) { return; }

            var deleter = new FolderDeleter();
            deleter.DeleteFolder(testTargetFolder.CakeFolder());
        }

        public void CreateCakeFolder(ITestTargetFolder testTargetFolder, out IErrorsAndInfos errorsAndInfos) {
            if (testTargetFolder.CakeFolder().Exists()) {
                errorsAndInfos = new ErrorsAndInfos();
                return;
            }

            vCakeInstaller.InstallCake(testTargetFolder.CakeFolder(), out errorsAndInfos);

            var gitUtilities = new GitUtilities();
            gitUtilities.DownloadReadyToCake(testTargetFolder.CakeFolder().SubFolder(@"tools"), errorsAndInfos);
        }
    }
}
