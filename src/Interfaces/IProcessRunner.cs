using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

public interface IProcessRunner {
    void RunProcess(string executableFullName, string arguments, IFolder workingFolder, IErrorsAndInfos errorsAndInfos);
}