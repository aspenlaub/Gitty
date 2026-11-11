using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class ShatilayaRunner(IFolderResolver folderResolver, IProcessRunner processRunner) : IShatilayaRunner {
    private const string _dotNetExecutableFileName = "dotnet";

    public async Task RunShatilayaAsync(IFolder repositoryFolder, string target, IErrorsAndInfos errorsAndInfos, CancellationToken cancellationToken) {
        IFolder shatilayaReleaseFolder = await folderResolver.ResolveAsync("$(Shatilaya)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return;
        }
        IFolder shatilayaWorkerFolder = await folderResolver.ResolveAsync("$(ShatilayaWorker)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return;
        }

        if (repositoryFolder.FullName.Contains("Shatilaya")) {
            var startInfo = new ProcessStartInfo {
                FileName = "robocopy.exe",
                Arguments = $"\"{shatilayaReleaseFolder.FullName}\" \"{shatilayaWorkerFolder.FullName}\" /E /R:2 /W:5 /MT:8",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            if (process == null) {
                errorsAndInfos.Errors.Add("Could not use robocopy");
                return;
            }
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode >= 8) {
                errorsAndInfos.Errors.Add($"Robocopy failed with exit code {process.ExitCode}");
                return;
            }
        } else {
            shatilayaWorkerFolder = shatilayaReleaseFolder;
        }

        var shatilayaAssemblyFullNames = Directory.GetFiles(shatilayaWorkerFolder.FullName, "Aspenlaub*Shatilaya*.dll", SearchOption.TopDirectoryOnly).ToList();
        if (shatilayaAssemblyFullNames.Count != 1) {
            errorsAndInfos.Errors.Add("Shatilaya assembly not found or not unique");
            return;
        }

        errorsAndInfos.Errors.Clear();
        errorsAndInfos.Infos.Clear();
        string shatilayaAssemblyName = shatilayaAssemblyFullNames[0].Replace(shatilayaWorkerFolder.FullName + '\\', "");
        string arguments = shatilayaAssemblyName + ' ' + (string.IsNullOrEmpty(target)
            ? $"--repository {repositoryFolder.FullName}"
            : $"--repository {repositoryFolder.FullName} --target {target}");
        await processRunner.RunProcessAsync(_dotNetExecutableFileName, arguments, shatilayaWorkerFolder, errorsAndInfos, cancellationToken);
        if (!errorsAndInfos.Infos.Any()) {
            errorsAndInfos.Errors.Add(Properties.Resources.ShatilayaDidNotLogAnything);
        }
    }
}