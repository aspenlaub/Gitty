using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test {
    [TestClass]
    public class GitPullTest {
        protected static ITestTargetFolder ChabTargetOne = new TestTargetFolder(nameof(GitPullTest), "Chab");
        protected static ITestTargetFolder ChabTargetTwo = new TestTargetFolder(nameof(GitPullTest) + "Copy", "Chab");
        protected static TestTargetInstaller TargetInstaller = new TestTargetInstaller(new CakeInstaller(new GitUtilities())); // ToDo: use IoC container
        protected static TestTargetRunner TargetRunner = new TestTargetRunner();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
            TargetInstaller.DeleteCakeFolder(ChabTargetTwo);
            TargetInstaller.CreateCakeFolder(ChabTargetTwo, out var errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            TargetInstaller.DeleteCakeFolder(ChabTargetTwo);
        }

        [TestInitialize]
        public void Initialize() {
            ChabTargetOne.Delete();
        }

        [TestCleanup]
        public void TestCleanup() {
            ChabTargetOne.Delete();
        }

        [TestMethod]
        public void LatestChangesArePulled() {
            var gitUtilities = new GitUtilities();
            var cakeRunner = new CakeRunner(new ProcessRunner());
            var errorsAndInfos = new ErrorsAndInfos();
            var url = "https://github.com/aspenlaub/" + ChabTargetOne.SolutionId + ".git";
            foreach (var target in new[] { ChabTargetOne, ChabTargetTwo }) {
                gitUtilities.Clone(url, target.Folder(), new CloneOptions { BranchName = "master" }, true, errorsAndInfos);
                cakeRunner.VerifyCakeVersion(target.Folder().SubFolder("tools"), errorsAndInfos);
                Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

                var addinsFolder = target.Folder().SubFolder("tools").SubFolder("Addins");
                if (!addinsFolder.Exists()) { continue; }

                var deleter = new FolderDeleter();
                deleter.DeleteFolder(addinsFolder);
            }

            // https://github.com/aspenlaub/Chab/commit/12fb5504d9380aabfc8d4c4ef2cf21117c810290 came before
            // https://github.com/aspenlaub/Chab/commit/480ac569f4fc1ce88d30cb990f670217a16f1f6f where OctoPack was disabled
            gitUtilities.Reset(ChabTargetOne.Folder(), "12fb5504d9380aabfc8d4c4ef2cf21117c810290", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var projectFile = ChabTargetOne.Folder().SubFolder("src").FullName + '\\' + ChabTargetOne.SolutionId + @".csproj";
            Assert.IsFalse(File.ReadAllText(projectFile).Contains("<RunOctoPack>false</RunOctoPack>"));
            Assert.IsTrue(File.ReadAllText(projectFile).Contains("<RunOctoPack>true</RunOctoPack>"));

            CakeBuildUtilities.CopyLatestBuildCakeScript(ChabTargetTwo, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());

            var buildCakeScriptFileName = ChabTargetTwo.FullName() + @"\" + "build.cake";
            var repositoryFolderSetStatement = "var repositoryFolder = MakeAbsolute(DirectoryPath.FromString(\"../../" + nameof(GitPullTest) + "/" + ChabTargetOne.SolutionId + "\")).FullPath;";
            var buildCakeScript = File.ReadAllLines(buildCakeScriptFileName).Select(s => s.Contains("var repositoryFolder =") ? repositoryFolderSetStatement : s);
            File.WriteAllLines(buildCakeScriptFileName, buildCakeScript);

            var solutionCakeFileFullName = ChabTargetTwo.Folder().FullName + @"\solution.cake";
            var solutionCakeContents = File.ReadAllText(solutionCakeFileFullName);
            solutionCakeContents = solutionCakeContents.Replace(@"./src", @"../../" + nameof(GitPullTest) + @"/" + ChabTargetOne.SolutionId + @"/src");
            File.WriteAllText(solutionCakeFileFullName, solutionCakeContents);

            TargetRunner.RunBuildCakeScript(ChabTargetTwo, new CakeRunner(new ProcessRunner()), "CleanRestorePull", errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsFalse(File.ReadAllText(projectFile).Contains("RunOctoPack"));
        }
    }
}
