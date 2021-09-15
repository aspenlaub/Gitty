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

        private readonly IProcessRunner ProcessRunner;
        private readonly IFolder WorkingFolder;

        public DotNetCakeInstaller(IProcessRunner processRunner) {
            ProcessRunner = processRunner;
            WorkingFolder = new Folder(Path.GetTempPath()).SubFolder(nameof(DotNetCakeInstaller));
            WorkingFolder.CreateIfNecessary();
        }

        public bool IsGlobalDotNetCakeInstalled(IErrorsAndInfos errorsAndInfos) {
            ProcessRunner.RunProcess(DotNetExecutableFileName, DotNetToolListArguments, WorkingFolder, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return false; }

            var line = errorsAndInfos.Infos.FirstOrDefault(l => l.StartsWith(CakeToolId));
            return line?.Substring(CakeToolId.Length).TrimStart().StartsWith(PinnedCakeToolVersion) == true;
        }

        public void InstallGlobalDotNetCakeIfNecessary(IErrorsAndInfos errorsAndInfos) {
            if (IsGlobalDotNetCakeInstalled(errorsAndInfos)) { return; }
            if (errorsAndInfos.AnyErrors()) { return; }

            ProcessRunner.RunProcess(DotNetExecutableFileName, DotNetInstallCakeToolArguments, WorkingFolder, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return; }

            if (IsGlobalDotNetCakeInstalled(errorsAndInfos)) { return; }
            errorsAndInfos.Errors.Add(Properties.Resources.CouldNotInstallCakeTool);
        }
    }
}
