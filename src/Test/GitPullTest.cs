using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Autofac;
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IContainer = Autofac.IContainer;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test {
    [TestClass]
    public class GitPullTest {
        protected static TestTargetFolder ChabStandardTarget = new TestTargetFolder(nameof(GitPullTest), "ChabStandard");
        protected static TestTargetInstaller TargetInstaller;
        protected static TestTargetRunner TargetRunner;
        private static IContainer vContainer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            vContainer = new ContainerBuilder().UseGittyDvinAndPegh(new DummyCsArgumentPrompter()).UseGittyTestUtilities().Build();
            TargetInstaller = vContainer.Resolve<TestTargetInstaller>();
            TargetRunner = vContainer.Resolve<TestTargetRunner>();
        }

        [TestInitialize]
        public void Initialize() {
            ChabStandardTarget.Delete();
        }

        [TestCleanup]
        public void TestCleanup() {
            ChabStandardTarget.Delete();
        }

        [TestMethod]
        public void CanPullLatestChanges() {
            var gitUtilities = vContainer.Resolve<IGitUtilities>();
            var cakeRunner = vContainer.Resolve<ICakeRunner>();
            var errorsAndInfos = new ErrorsAndInfos();
            var url = "https://github.com/aspenlaub/" + ChabStandardTarget.SolutionId + ".git";
            gitUtilities.Clone(url, "master", ChabStandardTarget.Folder(), new CloneOptions { BranchName = "master" }, true, errorsAndInfos);
            cakeRunner.VerifyCakeVersion(ChabStandardTarget.Folder().SubFolder("tools"), errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var addinsFolder = ChabStandardTarget.Folder().SubFolder("tools").SubFolder("Addins");
            if (addinsFolder.Exists()) {
                var deleter = new FolderDeleter();
                deleter.DeleteFolder(addinsFolder);
            }

            // https://github.com/aspenlaub/ChabStandard/commit/b8c4dee904e5748fce9aba8f912c37cf13f87a7c came before
            // https://github.com/aspenlaub/ChabStandard/commit/c6eb57b5ad242222f3aa95d8a936bd08fcbab299 where package reference to Microsoft.NET.Test.Sdk was added
            gitUtilities.Reset(ChabStandardTarget.Folder(), "b8c4dee904e5748fce9aba8f912c37cf13f87a7c", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var projectFile = ChabStandardTarget.Folder().SubFolder("src").SubFolder("Test").FullName + '\\' + ChabStandardTarget.SolutionId + @".Test.csproj";
            Assert.IsFalse(File.ReadAllText(projectFile).Contains("<PackageReference Include=\"Microsoft.NET.Test.Sdk\""));
            gitUtilities.Pull(ChabStandardTarget.Folder(), "UserName", "user.name@aspenlaub.org");

            Assert.IsTrue(File.ReadAllText(projectFile).Contains("<PackageReference Include=\"Microsoft.NET.Test.Sdk\""));
        }
    }
}
