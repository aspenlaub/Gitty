using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ICakeInstaller {
        /// <summary>
        /// Install cake
        /// </summary>
        /// <param name="toolsParentFolder">Folder with sub folder 'tools'. Cake.exe will be in 'tools/Cake'</param>
        /// <param name="errorsAndInfos"></param>
        void InstallCake(IFolder toolsParentFolder, out IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Return cake.exe full name
        /// </summary>
        /// <param name="toolsParentFolder">Folder with sub folder 'tools'. Cake.exe will be in 'tools/Cake'</param>
        /// <returns></returns>
        string CakeExeFileFullName(IFolder toolsParentFolder);

        /// <summary>
        /// Download and unpack PinnedCakeVersion (see CakeRunner) so that we do not need to install it using a powershell script
        /// </summary>
        /// <param name="toolsFolder">The folder called 'tools'. Cake.exe will be in 'tools/Cake'</param>
        /// <param name="errorsAndInfos"></param>
        void DownloadReadyToCake(IFolder toolsFolder, IErrorsAndInfos errorsAndInfos);
    }
}