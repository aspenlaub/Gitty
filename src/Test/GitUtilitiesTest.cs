using System.Collections.Generic;
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

#pragma warning disable CA1859

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test;

[TestClass]
public class GitUtilitiesTest {
    protected IFolder DevelopmentFolder, MasterFolder, NoGitFolder;
    private static readonly ITestTargetFolder _doNotPullFolder = new TestTargetFolder(nameof(GitUtilitiesTest) + @"DoNotPull", "Pakled");
    private static ITestTargetRunner _targetRunner;
    private static IContainer _container;
    private IGitUtilities _Sut;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context) {
        _container = new ContainerBuilder().UseGittyAndPegh("Gitty").UseGittyTestUtilities().Build();
        _targetRunner = _container.Resolve<ITestTargetRunner>();
    }

    [TestInitialize]
    public void Initialize() {
        _Sut = _container.Resolve<IGitUtilities>();
        IFolder checkOutFolder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(GitUtilitiesTest));
        DevelopmentFolder = checkOutFolder.SubFolder("Pakled-Development");
        MasterFolder = checkOutFolder.SubFolder("Pakled-Master");
        NoGitFolder = checkOutFolder.SubFolder("NoGit");
        _doNotPullFolder.Delete();

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
        foreach (IFolder folder in new[] { DevelopmentFolder, MasterFolder, NoGitFolder }.Where(folder => folder.Exists())) {
            deleter.DeleteFolder(folder);
        }

        _doNotPullFolder.Delete();
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

        const string url = "https://github.com/aspenlaub/Pakled.git";
        _Sut.Clone(url, branch, new Folder(folder.FullName), new CloneOptions { BranchName = branch }, true, errorsAndInfos);
    }

    [TestMethod]
    public void CanIdentifyCheckedOutBranch() {
        Assert.AreEqual("development", _Sut.CheckedOutBranch(DevelopmentFolder));
        IFolder developmentSubFolder = DevelopmentFolder.SubFolder("src").SubFolder("Test");
        Assert.AreEqual("development", _Sut.CheckedOutBranch(developmentSubFolder));
        Assert.AreEqual("master", _Sut.CheckedOutBranch(MasterFolder));
        Assert.AreEqual("", _Sut.CheckedOutBranch(NoGitFolder));
    }

    [TestMethod]
    public void CanGetHeadTipIdSha() {
        string headTipIdSha = _Sut.HeadTipIdSha(MasterFolder);
        Assert.IsFalse(string.IsNullOrEmpty(headTipIdSha));
        Assert.IsGreaterThanOrEqualTo(40, headTipIdSha.Length);
    }

    [TestMethod]
    public void CanDetermineUncommittedChanges() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.Contains(e => e.Contains(@"change.cs"), errorsAndInfos.Errors);
    }

    [TestMethod]
    public void CanUndoUncommittedChanges() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        File.WriteAllText(MasterFolder.FullName + @"\change.cs", @"This is not a change");
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.Contains(e => e.Contains(@"change.cs"), errorsAndInfos.Errors);
        errorsAndInfos = new ErrorsAndInfos();
        _Sut.Reset(MasterFolder, _Sut.HeadTipIdSha(MasterFolder), errorsAndInfos);
        _Sut.VerifyThatThereAreNoUncommittedChanges(MasterFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
    }

    [TestMethod]
    public void CanCheckIfIsBranchAheadOfOrBehindMaster() {
        var errorsAndInfos = new ErrorsAndInfos();
        CloneRepository(_doNotPullFolder.Folder(), "do-not-pull-from-me", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsFalse(_Sut.IsBranchAheadOfMaster(MasterFolder));
        _container.Resolve<IEmbeddedCakeScriptCopier>().CopyCakeScriptEmbeddedInAssembly(Assembly.GetExecutingAssembly(), BuildCake.Standard, _doNotPullFolder, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        _targetRunner.RunBuildCakeScript(BuildCake.Standard, _doNotPullFolder, "CleanRestorePull", errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsTrue(_Sut.IsBranchAheadOfMaster(_doNotPullFolder.Folder()));
    }

    [TestMethod]
    public void CanIdentifyUrlOwnerAndName() {
        var errorsAndInfos = new ErrorsAndInfos();
        _Sut.IdentifyOwnerAndName(MasterFolder, out string owner, out string name, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.AreEqual("aspenlaub", owner);
        Assert.AreEqual("Pakled", name);
    }

    [TestMethod]
    public void CanGetAllIdShas() {
        IList<string> allIdShas = _Sut.AllIdShas(MasterFolder);
        Assert.IsGreaterThan(50, allIdShas.Count);
        Assert.Contains(_Sut.HeadTipIdSha(MasterFolder), allIdShas);
    }

}