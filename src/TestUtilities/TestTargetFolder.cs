using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

public class TestTargetFolder : ITestTargetFolder {
    public string TestClassId { get; }
    public string SolutionId { get; }

    public TestTargetFolder(string testClassId, string solutionId) {
        TestClassId = testClassId;
        SolutionId = solutionId;
    }

    public IFolder Folder() {
        return new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(TestClassId).SubFolder(SolutionId);
    }

    public bool Exists() {
        return Folder().Exists();
    }

    public string FullName() {
        return Folder().FullName;
    }

    public IFolder MasterBinFolder() {
        return Folder().ParentFolder().SubFolder(SolutionId + @"Bin");
    }

    public IFolder MasterDebugBinFolder() {
        return Folder().ParentFolder().SubFolder(SolutionId + @"Bin/Debug");
    }

    public IFolder MasterReleaseBinFolder() {
        return Folder().ParentFolder().SubFolder(SolutionId + @"Bin/Release");
    }

    public void Delete() {
        var deleter = new FolderDeleter();

        if (Exists()) {
            deleter.DeleteFolder(Folder());
        }

        var folder = MasterBinFolder();
        if (folder.Exists()) {
            deleter.DeleteFolder(folder);
        }
    }
}