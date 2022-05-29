using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class DotNetCakeRunner : IDotNetCakeRunner {
    private const string DotNetExecutableFileName = "dotnet";

    private readonly IProcessRunner ProcessRunner;

    public DotNetCakeRunner(IProcessRunner processRunner) {
        ProcessRunner = processRunner;
    }

    public void CallCake(string scriptFileFullName, IErrorsAndInfos errorsAndInfos) {
        CallCake(scriptFileFullName, "", errorsAndInfos);
    }

    public void CallCake(string scriptFileFullName, string target, IErrorsAndInfos errorsAndInfos) {
        if (!File.Exists(scriptFileFullName)) {
            errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FileNotFound, scriptFileFullName));
            return;
        }

        var scriptFileFolder = new Folder(scriptFileFullName.Substring(0, scriptFileFullName.LastIndexOf('\\')));
        var arguments = "cake \"" + scriptFileFullName + "\"";
        if (target != "") {
            arguments = arguments + " --target \"" + target + "\"";
        }
        ProcessRunner.RunProcess(DotNetExecutableFileName, arguments, scriptFileFolder, errorsAndInfos);
    }
}