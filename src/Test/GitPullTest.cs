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
        protected static TestTargetFolder ChabTarget = new(nameof(GitPullTest), "Chab");
        protected static ITestTargetRunner TargetRunner;
        private static IContainer Container;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            Container = new ContainerBuilder().UseGittyAndPegh(new DummyCsArgumentPrompter()).UseGittyTestUtilities().Build();
            TargetRunner = Container.Resolve<ITestTargetRunner>();
        }

        [TestInitialize]
        public void Initialize() {
            ChabTarget.Delete();
        }

        [TestCleanup]
        public void TestCleanup() {
            ChabTarget.Delete();
        }

        [TestMethod]
        public void CanPullLatestChanges() {
            var gitUtilities = Container.Resolve<IGitUtilities>();
            var errorsAndInfos = new ErrorsAndInfos();
            var url = "https://github.com/aspenlaub/" + ChabTarget.SolutionId + ".git";
            gitUtilities.Clone(url, "master", ChabTarget.Folder(), new CloneOptions { BranchName = "master" }, true, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var addinsFolder = ChabTarget.Folder().SubFolder("tools").SubFolder("Addins");
            if (addinsFolder.Exists()) {
                var deleter = new FolderDeleter();
                deleter.DeleteFolder(addinsFolder);
            }

            // https://github.com/aspenlaub/ChabStandard/commit/b8c4dee904e5748fce9aba8f912c37cf13f87a7c came before
            // https://github.com/aspenlaub/ChabStandard/commit/c6eb57b5ad242222f3aa95d8a936bd08fcbab299 where package reference to Microsoft.NET.Test.Sdk was added
            gitUtilities.Reset(ChabTarget.Folder(), "b8c4dee904e5748fce9aba8f912c37cf13f87a7c", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var projectFile = ChabTarget.Folder().SubFolder("src").SubFolder("Test").FullName + '\\' + ChabTarget.SolutionId + @"Standard.Test.csproj";
            Assert.IsFalse(File.ReadAllText(projectFile).Contains("<PackageReference Include=\"Microsoft.NET.Test.Sdk\""));
            gitUtilities.Pull(ChabTarget.Folder(), "UserName", "user.name@aspenlaub.org");

            projectFile = projectFile.Replace("Standard", "");
            Assert.IsTrue(File.ReadAllText(projectFile).Contains("<PackageReference Include=\"Microsoft.NET.Test.Sdk\""));
        }
    }
}
