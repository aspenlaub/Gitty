using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using LibGit2Sharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IContainer = Autofac.IContainer;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test;

[TestClass]
public class GitHubUtilitiesTest {
    protected IFolder PakledMasterFolder, PakledDevelopmentFolder, DvinMasterFolder;
    private static IContainer _container;
    private IGitUtilities _GitUtilities;

    [TestInitialize]
    public void Initialize() {
        _container = new ContainerBuilder().UseGittyAndPegh("Gitty", new DummyCsArgumentPrompter()).UseGittyTestUtilities().Build();
        _GitUtilities = _container.Resolve<IGitUtilities>();
        IFolder checkOutFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(GitHubUtilitiesTest));
        PakledMasterFolder = checkOutFolder.SubFolder("PakledCore-Master");
        PakledDevelopmentFolder = checkOutFolder.SubFolder("PakledCore-Development");
        DvinMasterFolder = checkOutFolder.SubFolder("Dvin-Master");

        CleanUp();
        var errorsAndInfos = new ErrorsAndInfos();
        CloneRepository("Pakled", PakledMasterFolder, "master", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        CloneRepository("Pakled", PakledDevelopmentFolder, "do-not-pull-from-me", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
    }

    [TestCleanup]
    public void CleanUp() {
        var deleter = new FolderDeleter();
        foreach (IFolder folder in new[] { PakledMasterFolder, PakledDevelopmentFolder, DvinMasterFolder }.Where(folder => folder.Exists())) {
            deleter.DeleteFolder(folder);
        }
    }

    private void CloneRepository(string repositoryId, IFolder folder, string branch, IErrorsAndInfos errorsAndInfos) {
        if (folder.GitSubFolder().Exists()) {
            return;
        }

        if (folder.Exists()) {
            var deleter = new FolderDeleter();
            Assert.IsTrue(deleter.CanDeleteFolder(folder));
            deleter.DeleteFolder(folder);
        }

        string url = $"https://github.com/aspenlaub/{repositoryId}.git";
        _GitUtilities.Clone(url, branch, new Folder(folder.FullName), new CloneOptions { BranchName = branch }, true, errorsAndInfos);
    }

    [TestMethod]
    public async Task CanCheckIfPullRequestsExist() {
        IGitHubUtilities sut = _container.Resolve<IGitHubUtilities>();
        var errorsAndInfos = new ErrorsAndInfos();
        YesNoInconclusive hasOpenPullRequest = await HasOpenPullRequestAsync(sut, "", errorsAndInfos);
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

        YesNoInconclusive hasPullRequest = await HasPullRequestForThisBranchAndItsHeadTipAsync(sut, errorsAndInfos);
        if (hasOpenPullRequest.Inconclusive) { return; }

        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsTrue(hasPullRequest.YesNo);
    }

    protected async Task<YesNoInconclusive> HasOpenPullRequestAsync(IGitHubUtilities sut, string semicolonSeparatedListOfPullRequestNumbersToIgnore, ErrorsAndInfos errorsAndInfos) {
        bool inconclusive = false;
        bool hasOpenPullRequest = false;
        try {
            hasOpenPullRequest = await sut.HasOpenPullRequestAsync(PakledMasterFolder, semicolonSeparatedListOfPullRequestNumbersToIgnore, errorsAndInfos);
        } catch (WebException) {
            inconclusive = true;
        }

        return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
    }

    protected async Task<YesNoInconclusive> HasOpenPullRequestForThisBranchAsync(IGitHubUtilities sut, bool master, ErrorsAndInfos errorsAndInfos) {
        bool inconclusive = false;
        bool hasOpenPullRequest = false;
        try {
            hasOpenPullRequest = await sut.HasOpenPullRequestForThisBranchAsync(master ? PakledMasterFolder : PakledDevelopmentFolder, errorsAndInfos);
        } catch (WebException) {
            inconclusive = true;
        }

        return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
    }

    protected async Task<YesNoInconclusive> HasPullRequestForThisBranchAndItsHeadTipAsync(IGitHubUtilities sut, ErrorsAndInfos errorsAndInfos) {
        bool inconclusive = false;
        bool hasOpenPullRequest = false;
        try {
            hasOpenPullRequest = await sut.HasPullRequestForThisBranchAndItsHeadTipAsync(PakledDevelopmentFolder, errorsAndInfos);
        } catch (WebException) {
            inconclusive = true;
        }

        return new YesNoInconclusive { YesNo = hasOpenPullRequest, Inconclusive = inconclusive };
    }

    [TestMethod]
    public async Task CanCheckHowManyPullRequestsExist() {
        IGitHubUtilities sut = _container.Resolve<IGitHubUtilities>();
        var errorsAndInfos = new ErrorsAndInfos();
        try {
            int numberOfPullRequests = await sut.GetNumberOfPullRequestsAsync(PakledMasterFolder, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
            Assert.IsGreaterThan(0, numberOfPullRequests);
        } catch (WebException) {
        }
    }

    [TestMethod]
    public void CanCloneDvin() {
        CleanUp();
        var errorsAndInfos = new ErrorsAndInfos();
        CloneRepository("Dvin", DvinMasterFolder, "master", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
    }
}