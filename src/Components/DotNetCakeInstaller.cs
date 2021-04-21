using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components {
    public class DotNetCakeInstaller : IDotNetCakeInstaller {
        private const string CakeToolId = "cake.tool";
        private const string PinnedCakeToolVersion = "1.1.0";
        private const string DotNetExecutableFileName = "dotnet";
        private const string DotNetToolListArguments = "tool list --global";
        private const string DotNetInstallCakeToolArguments = "tool install Cake.Tool --version 1.1.0 --global";

        private readonly IProcessRunner vProcessRunner;
        private readonly IFolder vWorkingFolder;

        public DotNetCakeInstaller(IProcessRunner processRunner) {
            vProcessRunner = processRunner;
            vWorkingFolder = new Folder(Path.GetTempPath()).SubFolder(nameof(DotNetCakeInstaller));
            vWorkingFolder.CreateIfNecessary();
        }

        public bool IsGlobalDotNetCakeInstalled(IErrorsAndInfos errorsAndInfos) {
            vProcessRunner.RunProcess(DotNetExecutableFileName, DotNetToolListArguments, vWorkingFolder, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return false; }

            var line = errorsAndInfos.Infos.FirstOrDefault(l => l.StartsWith(CakeToolId));
            return line?.Substring(CakeToolId.Length).TrimStart().StartsWith(PinnedCakeToolVersion) == true;
        }

        public void InstallGlobalDotNetCakeIfNecessary(IErrorsAndInfos errorsAndInfos) {
            if (IsGlobalDotNetCakeInstalled(errorsAndInfos)) { return; }
            if (errorsAndInfos.AnyErrors()) { return; }

            vProcessRunner.RunProcess(DotNetExecutableFileName, DotNetInstallCakeToolArguments, vWorkingFolder, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return; }

            if (IsGlobalDotNetCakeInstalled(errorsAndInfos)) { return; }
            errorsAndInfos.Errors.Add(Properties.Resources.CouldNotInstallCakeTool);
        }
    }
}
