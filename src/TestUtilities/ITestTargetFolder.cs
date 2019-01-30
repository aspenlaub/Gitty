using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public interface ITestTargetFolder {
        string TestClassId { get; }
        string SolutionId { get; }
        IFolder Folder();
        bool Exists();
        string FullName();
        IFolder MasterDebugBinFolder();
        IFolder MasterReleaseBinFolder();
        void Delete();
        IFolder CakeFolder();
    }
}