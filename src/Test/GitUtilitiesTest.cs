using System.IO;
using System.Linq;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IContainer = Autofac.IContainer;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test {
    [TestClass]
    public class GitUtilitiesTest {
        protected IFolder DevelopmentFolder, MasterFolder, NoGitFolder;
        protected static ITestTargetFolder DoNotPullFolder = new TestTargetFolder(nameof(GitUtilitiesTest) + @"DoNotPull", "PakledCore");
        protected static TestTargetInstaller TargetInstaller;
        protected static TestTargetRunner TargetRunner;
        private static IContainer vContainer;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            vContainer = new ContainerBuilder().UseGitty().UseGittyTestUtilities().Build();
            TargetInstaller = vContainer.Resolve<TestTargetInstaller>();
            TargetRunner = vContainer.Resolve<TestTargetRunner>();
            TargetInstaller.DeleteCakeFolder(DoNotPullFolder);
            TargetInstaller.CreateCakeFolder(DoNotPullFolder, out var errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            TargetInstaller.DeleteCakeFolder(DoNotPullFolder);
        }

        [TestInitialize]
        public void Initialize() {
            var checkOutFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(GitUtilitiesTest));
            DevelopmentFolder = checkOutFolder.SubFolder("PakledCore-Development");
            MasterFolder = checkOutFolder.SubFolder("PakledCore-Master");
            NoGitFolder = checkOutFolder.SubFolder("NoGit");
            DoNotPullFolder.Delete();

            CleanUp();
            CloneRepository(MasterFolder, "master");
            CloneRepository(DevelopmentFolder, "development");
            if (!NoGitFolder.Exists()) {
                Directory.CreateDirectory(NoGitFolder.FullName);
            }
        }

        [TestCleanup]
        public void CleanUp() {
            var deleter = new FolderDeleter();
            foreach (var folder in new[] { DevelopmentFolder, MasterFolder, NoGitFolder }.Where(folder => folder.Exists())) {
                deleter.DeleteFolder(folder);
            }

            DoNotPullFolder.Delete();
        }

        private static void CloneRepository(IFolder folder, string branch) {
            if (folder.GitSubFolder().Exists()) {
                return;
            }

            if (folder.Exists()) {
                var deleter = new FolderDeleter();
                Assert.IsTrue(deleter.CanDeleteFolder(folder));
                deleter.DeleteFolder(folder);
            }

            const string url = "https://github.com/aspenlaub/PakledCore.git";
            Repository.Clone(url, folder.FullName, new CloneOptions { BranchName = branch });
        }

        [TestMethod]
        public void CanIdentifyCheckedOutBranch() {
            var sut = vContainer.Resolve<IGitUtilities>();
            Assert.AreEqual("development", sut.CheckedOutBranch(DevelopmentFolder));
            var developmentSubFolder = DevelopmentFolder.SubFolder("src").SubFolder("Test");
            Assert.AreEqual("development", sut.CheckedOutBranch(developmentSubFolder));
            Assert.AreEqual("master", sut.CheckedOutBranch(MasterFolder));
            Assert.AreEqual("", sut.CheckedOutBranch(NoGitFolder));
        }

        [TestMethod]
        public void CanGetHeadTipIdSha() {
            var sut = vContainer.Resolve<IGitUtilities>();
            var headTipIdSha = sut.HeadTipIdSha(MasterFolder);
            Assert.IsFalse(string.IsNullOrEmpty(headTipIdSha));
            Assert.IsTrue(headTipIdSha.Length >= 40);
        }

        [TestMethod]
        public void CanDetermineUncommittedChanges() {
            var sut = vContainer.Resolve<IGitUtilities>();
            var errorsAndInfos = new ErrorsAndInfos();
            sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
            sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
        }

        [TestMethod]
        public void CanUndoUncommittedChanges() {
            var sut = vContainer.Resolve<IGitUtilities>();
            var errorsAndInfos = new ErrorsAndInfos();
            sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
            sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
            errorsAndInfos = new ErrorsAndInfos();
            sut.Reset(MasterFolder, sut.HeadTipIdSha(MasterFolder), errorsAndInfos);
            sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        }

        [TestMethod]
        public void CanCheckIfIsBranchAheadOfMaster() {
            CloneRepository(DoNotPullFolder.Folder(), "do-not-pull-from-me");
            var sut = vContainer.Resolve<IGitUtilities>();
            Assert.IsFalse(sut.IsBranchAheadOfMaster(MasterFolder));
            var errorsAndInfos = new ErrorsAndInfos();
            vContainer.Resolve<CakeBuildUtilities>().CopyCakeScriptEmbeddedInAssembly(Assembly.GetExecutingAssembly(), BuildCake.Standard, DoNotPullFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            TargetRunner.RunBuildCakeScript(BuildCake.Standard, DoNotPullFolder, "CleanRestorePull", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsTrue(sut.IsBranchAheadOfMaster(DoNotPullFolder.Folder()));
        }

        [TestMethod]
        public void CanIdentifyUrlOwnerAndName() {
            var sut = new GitUtilities();
            var errorsAndInfos = new ErrorsAndInfos();
            sut.IdentifyOwnerAndName(MasterFolder, out var owner, out var name, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.AreEqual("aspenlaub", owner);
            Assert.AreEqual("PakledCore", name);
        }
    }
}
