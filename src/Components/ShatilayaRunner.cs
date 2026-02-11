using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class ShatilayaRunner(IFolderResolver folderResolver, IProcessRunner processRunner) : IShatilayaRunner {
    public async Task RunShatilayaAsync(IFolder repositoryFolder, string target, IErrorsAndInfos errorsAndInfos) {
        IFolder workingFolder = await folderResolver.ResolveAsync("$(Shatilaya)", errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return;
        }
        var executableFullNames = Directory.GetFiles(workingFolder.FullName, "*Shatilaya*.exe").ToList();
        if (executableFullNames.Count != 1) {
            errorsAndInfos.Errors.Add("Shatilaya executable not found or not unique");
            return;
        }

        errorsAndInfos.Errors.Clear();
        errorsAndInfos.Infos.Clear();
        string executableFullName = executableFullNames[0];
        string arguments = string.IsNullOrEmpty(target)
        ? $"--repository {repositoryFolder.FullName}"
        : $"--repository {repositoryFolder.FullName} --target {target}";
        processRunner.RunProcess(executableFullName, arguments, workingFolder, errorsAndInfos);
        if (!errorsAndInfos.Infos.Any()) {
            errorsAndInfos.Errors.Add(Properties.Resources.ShatilayaDidNotLogAnything);
        }
    }
}