using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ICakeInstaller {
        void InstallCake(IFolder cakeFolder, out IErrorsAndInfos errorsAndInfos);
        string CakeExeFileFullName(IFolder cakeFolder);
    }
}