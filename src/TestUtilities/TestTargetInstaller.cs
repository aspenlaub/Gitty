using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class TestTargetInstaller {
        private readonly ICakeInstaller vCakeInstaller;
        private readonly IGitUtilities vGitUtilities;

        public TestTargetInstaller(ICakeInstaller cakeInstaller, IGitUtilities gitUtilities) {
            vCakeInstaller = cakeInstaller;
            vGitUtilities = gitUtilities;
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

            vGitUtilities.DownloadReadyToCake(testTargetFolder.CakeFolder().SubFolder(@"tools"), errorsAndInfos);
        }
    }
}
