using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class ShatilayaRunner(IFolderResolver folderResolver, IProcessRunner processRunner) : IShatilayaRunner {
    private const string _dotNetExecutableFileName = "dotnet";

    public async Task RunShatilayaAsync(IFolder repositoryFolder, string target, IErrorsAndInfos errorsAndInfos) {
        IFolder workingFolder = await folderResolver.ResolveAsync("$(Shatilaya)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return;
        }
        var shatilayaAssemblyFullNames = Directory.GetFiles(workingFolder.FullName, "*Shatilaya*.dll").ToList();
        if (shatilayaAssemblyFullNames.Count != 1) {
            errorsAndInfos.Errors.Add("Shatilaya assembly not found or not unique");
            return;
        }

        errorsAndInfos.Errors.Clear();
        errorsAndInfos.Infos.Clear();
        string shatilayaAssemblyName = shatilayaAssemblyFullNames[0].Replace(workingFolder.FullName + '\\', "");
        string arguments = shatilayaAssemblyName + ' ' + (string.IsNullOrEmpty(target)
        ? $"--repository {repositoryFolder.FullName}"
        : $"--repository {repositoryFolder.FullName} --target {target}");
        processRunner.RunProcess(_dotNetExecutableFileName, arguments, workingFolder, errorsAndInfos);
        if (!errorsAndInfos.Infos.Any()) {
            errorsAndInfos.Errors.Add(Properties.Resources.ShatilayaDidNotLogAnything);
        }
    }
}