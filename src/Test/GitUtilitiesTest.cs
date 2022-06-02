using System.IO;
using System.Linq;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
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
public class GitUtilitiesTest {
    protected IFolder DevelopmentFolder, MasterFolder, NoGitFolder;
    protected static ITestTargetFolder DoNotPullFolder = new TestTargetFolder(nameof(GitUtilitiesTest) + @"DoNotPull", "PakledCore");
    protected static ITestTargetRunner TargetRunner;
    private static IContainer Container;
    private IGitUtilities _Sut;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context) {
        Container = new ContainerBuilder().UseGittyAndPegh("Gitty", new DummyCsArgumentPrompter()).UseGittyTestUtilities().Build();
        TargetRunner = Container.Resolve<ITestTargetRunner>();
    }

    [TestInitialize]
    public void Initialize() {
        _Sut = Container.Resolve<IGitUtilities>();
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
        _Sut.Clone(url, branch, new Folder(folder.FullName), new CloneOptions { BranchName = branch }, true, errorsAndInfos);
    }

    [TestMethod]
    public void CanIdentifyCheckedOutBranch() {
        Assert.AreEqual("development", _Sut.CheckedOutBranch(DevelopmentFolder));
        var developmentSubFolder = DevelopmentFolder.SubFolder("src").SubFolder("Test");
        Assert.AreEqual("development", _Sut.CheckedOutBranch(developmentSubFolder));
        Assert.AreEqual("master", _Sut.CheckedOutBranch(MasterFolder));
        Assert.AreEqual("", _Sut.CheckedOutBranch(NoGitFolder));
    }

    [TestMethod]
    public void CanGetHeadTipIdSha() {
        var headTipIdSha = _Sut.HeadTipIdSha(MasterFolder);
        Assert.IsFalse(string.IsNullOrEmpty(headTipIdSha));
        Assert.IsTrue(headTipIdSha.Length >= 40);
    }

    [TestMethod]
    public void CanDetermineUncommittedChanges() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
    }

    [TestMethod]
    public void CanUndoUncommittedChanges() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains(@"change.cs")));
        errorsAndInfos = new ErrorsAndInfos();
        _Sut.Reset(MasterFolder, _Sut.HeadTipIdSha(MasterFolder), errorsAndInfos);
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
    }

    [TestMethod]
    public void CanCheckIfIsBranchAheadOfMaster() {
        var errorsAndInfos = new ErrorsAndInfos();
        CloneRepository(DoNotPullFolder.Folder(), "do-not-pull-from-me", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsFalse(_Sut.IsBranchAheadOfMaster(MasterFolder));
        Container.Resolve<IEmbeddedCakeScriptCopier>().CopyCakeScriptEmbeddedInAssembly(Assembly.GetExecutingAssembly(), BuildCake.Standard, DoNotPullFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        TargetRunner.RunBuildCakeScript(BuildCake.Standard, DoNotPullFolder, "CleanRestorePull", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsTrue(_Sut.IsBranchAheadOfMaster(DoNotPullFolder.Folder()));
    }

    [TestMethod]
    public void CanIdentifyUrlOwnerAndName() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.IdentifyOwnerAndName(MasterFolder, out var owner, out var name, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.AreEqual("aspenlaub", owner);
        Assert.AreEqual("PakledCore", name);
    }

    [TestMethod]
    public void CanGetAllIdShas() {
        var allIdShas = _Sut.AllIdShas(MasterFolder);
        Assert.IsTrue(allIdShas.Count > 50);
        Assert.IsTrue(allIdShas.Contains(_Sut.HeadTipIdSha(MasterFolder)));
    }

}