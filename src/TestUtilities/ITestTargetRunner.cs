using System.Reflection;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

public interface ITestTargetRunner {
    Task RunShatilayaAsync(ITestTargetFolder testTargetFolder, string target, IErrorsAndInfos errorsAndInfos);
    Task IgnorePendingChangesAndDoNotPushAsync(Assembly assembly, ITestTargetFolder testeTargetFolder, IErrorsAndInfos errorsAndInfos);
}