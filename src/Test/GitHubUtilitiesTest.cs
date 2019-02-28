using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    public class GitHubUtilitiesTest {
        protected IFolder MasterFolder, DevelopmentFolder;
        private static IContainer vContainer;

        [TestInitialize]
        public void Initialize() {
            vContainer = new ContainerBuilder().UseGitty().UseGittyTestUtilities().Build();
            var checkOutFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(GitHubUtilitiesTest));
            MasterFolder = checkOutFolder.SubFolder("PakledCore-Master");
            DevelopmentFolder = checkOutFolder.SubFolder("PakledCore-Development");

            CleanUp();
            CloneRepository(MasterFolder, "master");
            CloneRepository(DevelopmentFolder, "do-not-pull-from-me");
        }

        [TestCleanup]
        public void CleanUp() {
            var deleter = new FolderDeleter();
            foreach (var folder in new[] { MasterFolder, DevelopmentFolder }.Where(folder => folder.Exists())) {
                deleter.DeleteFolder(folder);
            }
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
        public async Task CanCheckIfPullRequestsExist() {
            var sut = vContainer.Resolve<IGitHubUtilities>();
            var errorsAndInfos = new ErrorsAndInfos();
            var hasOpenPullRequest = await HasOpenPullRequestAsync(sut, "", errorsAndInfos);
            if (hasOpenPullRequest.Inconclusive) { return; }

            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsTrue(hasOpenPullRequest.YesNo);

            hasOpenPullRequest = await HasOpenPullRequestAsync(sut, "1", errorsAndInfos);
            if (hasOpenPullRequest.Inconclusive) { return; }

            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsFalse(hasOpenPullRequest.YesNo);

            hasOpenPullRequest = await HasOpenPullRequestForThisBranchAsync(sut, true, errorsAndInfos);
            if (hasOpenPullRequest.Inconclusive) { return; }

            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsFalse(hasOpenPullRequest.YesNo);

            hasOpenPullRequest = await HasOpenPullRequestForThisBranchAsync(sut, false, errorsAndInfos);
            if (hasOpenPullRequest.Inconclusive) { return; }

            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsTrue(hasOpenPullRequest.YesNo);

            var hasPullRequest = await HasPullRequestForThisBranchAndItsHeadTipAsync(sut, errorsAndInfos);
            if (hasOpenPullRequest.Inconclusive) { return; }

            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsTrue(hasPullRequest.YesNo);
        }

        protected async Task<YesNoInconclusive> HasOpenPullRequestAsync(IGitHubUtilities sut, string semicolonSeparatedListOfPullRequestNumbersToIgnore, ErrorsAndInfos errorsAndInfos) {
            var inconclusive = false;
            var hasOpenPullRequest = false;
            try {
                hasOpenPullRequest = await sut.HasOpenPullRequestAsync(MasterFolder, semicolonSeparatedListOfPullRequestNumbersToIgnore, errorsAndInfos);
            } catch (WebException) {
                inconclusive = true;
            }

            return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
        }

        protected async Task<YesNoInconclusive> HasOpenPullRequestForThisBranchAsync(IGitHubUtilities sut, bool master, ErrorsAndInfos errorsAndInfos) {
            var inconclusive = false;
            var hasOpenPullRequest = false;
            try {
                hasOpenPullRequest = await sut.HasOpenPullRequestForThisBranchAsync(master ? MasterFolder : DevelopmentFolder, errorsAndInfos);
            } catch (WebException) {
                inconclusive = true;
            }

            return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
        }

        protected async Task<YesNoInconclusive> HasPullRequestForThisBranchAndItsHeadTipAsync(IGitHubUtilities sut, ErrorsAndInfos errorsAndInfos) {
            var inconclusive = false;
            var hasOpenPullRequest = false;
            try {
                hasOpenPullRequest = await sut.HasPullRequestForThisBranchAndItsHeadTipAsync(DevelopmentFolder, errorsAndInfos);
            } catch (WebException) {
                inconclusive = true;
            }

            return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
        }

        [TestMethod]
        public async Task CanCheckHowManyPullRequestsExist() {
            var sut = vContainer.Resolve<IGitHubUtilities>();
            var errorsAndInfos = new ErrorsAndInfos();
            try {
                var numberOfPullRequests = await sut.GetNumberOfPullRequestsAsync(MasterFolder, errorsAndInfos);
                Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
                Assert.IsTrue(numberOfPullRequests > 0);
            } catch (WebException) {
            }
        }
    }
}
