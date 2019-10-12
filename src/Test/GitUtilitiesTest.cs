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
        private IGitUtilities vSut;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            vContainer = new ContainerBuilder().UseGittyDvinAndPegh(new DummyCsArgumentPrompter()).UseGittyTestUtilities().Build();
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
            vSut = vContainer.Resolve<IGitUtilities>();
            var checkOutFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(GitUtilitiesTest));
            DevelopmentFolder = checkOutFolder.SubFolder("PakledCore-Development");
            MasterFolder = checkOutFolder.SubFolder("PakledCore-Master");
            NoGitFolder = checkOutFolder.SubFolder("NoGit");
            DoNotPullFolder.Delete();

            CleanUp();
            var errorsAndInfos = new ErrorsAndInfos();
            CloneRepository(MasterFolder, "master", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            CloneRepository(DevelopmentFolder, "development", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
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

        private void CloneRepository(IFolder folder, string branch, IErrorsAndInfos errorsAndInfos) {
            if (folder.GitSubFolder().Exists()) {
                return;
            }

            if (folder.Exists()) {
                var deleter = new FolderDeleter();
                Assert.IsTrue(deleter.CanDeleteFolder(folder));
                deleter.DeleteFolder(folder);
            }

            const string url = "https://github.com/aspenlaub/PakledCore.git";
            vSut.Clone(url, branch, new Folder(folder.FullName), new CloneOptions { BranchName = branch }, true, errorsAndInfos);
        }

        [TestMethod]
        public void CanIdentifyCheckedOutBranch() {
            Assert.AreEqual("development", vSut.CheckedOutBranch(DevelopmentFolder));
            var developmentSubFolder = DevelopmentFolder.SubFolder("src").SubFolder("Test");
            Assert.AreEqual("development", vSut.CheckedOutBranch(developmentSubFolder));
            Assert.AreEqual("master", vSut.CheckedOutBranch(MasterFolder));
            Assert.AreEqual("", vSut.CheckedOutBranch(NoGitFolder));
        }

        [TestMethod]
        public void CanGetHeadTipIdSha() {
            var headTipIdSha = vSut.HeadTipIdSha(MasterFolder);
            Assert.IsFalse(string.IsNullOrEmpty(headTipIdSha));
            Assert.IsTrue(headTipIdSha.Length >= 40);
        }

        [TestMethod]
        public void CanDetermineUncommittedChanges() {
            var errorsAndInfos = new ErrorsAndInfos();
            vSut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
            vSut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
        }

        [TestMethod]
        public void CanUndoUncommittedChanges() {
            var errorsAndInfos = new ErrorsAndInfos();
            vSut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
            vSut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
            errorsAndInfos = new ErrorsAndInfos();
            vSut.Reset(MasterFolder, vSut.HeadTipIdSha(MasterFolder), errorsAndInfos);
            vSut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        }

        [TestMethod]
        public void CanCheckIfIsBranchAheadOfMaster() {
            var errorsAndInfos = new ErrorsAndInfos();
            CloneRepository(DoNotPullFolder.Folder(), "do-not-pull-from-me", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsFalse(vSut.IsBranchAheadOfMaster(MasterFolder));
            vContainer.Resolve<CakeBuildUtilities>().CopyCakeScriptEmbeddedInAssembly(Assembly.GetExecutingAssembly(), BuildCake.Standard, DoNotPullFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            TargetRunner.RunBuildCakeScript(BuildCake.Standard, DoNotPullFolder, "CleanRestorePull", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsTrue(vSut.IsBranchAheadOfMaster(DoNotPullFolder.Folder()));
        }

        [TestMethod]
        public void CanIdentifyUrlOwnerAndName() {
            var errorsAndInfos = new ErrorsAndInfos();
            vSut.IdentifyOwnerAndName(MasterFolder, out var owner, out var name, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.AreEqual("aspenlaub", owner);
            Assert.AreEqual("PakledCore", name);
        }
    }
}
