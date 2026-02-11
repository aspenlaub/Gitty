using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

public interface IShatilayaRunner {
#pragma warning disable IDE0051
    Task RunShatilayaAsync(IFolder repositoryFolder, string target, IErrorsAndInfos errorsAndInfos);
#pragma warning restore IDE0051
}