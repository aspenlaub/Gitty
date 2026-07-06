using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

public interface IProcessRunner {
    Task RunProcessAsync(string executableFileName, string arguments, IFolder workingFolder,
        IErrorsAndInfos errorsAndInfos, CancellationToken cancellationToken);
}