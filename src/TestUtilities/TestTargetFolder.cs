using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
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

            foreach (var folder in new[] { MasterDebugBinFolder(), MasterReleaseBinFolder() }.Where(folder => folder.Exists())) {
                deleter.DeleteFolder(folder);
            }
        }

        public IFolder CakeFolder() {
            return new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(TestClassId).SubFolder("Cake");
        }
    }
}
