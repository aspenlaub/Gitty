using System.Reflection;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

public class TestTargetRunner(IShatilayaRunner shatilayaRunner) : ITestTargetRunner {
    public async Task RunShatilayaAsync(ITestTargetFolder testTargetFolder, string target, IErrorsAndInfos errorsAndInfos) {
        await shatilayaRunner.RunShatilayaAsync(testTargetFolder.Folder(), target, errorsAndInfos);
    }

    public async Task IgnorePendingChangesAndDoNotPushAsync(Assembly assembly, ITestTargetFolder testeTargetFolder, IErrorsAndInfos errorsAndInfos) {
        await shatilayaRunner.RunShatilayaAsync(testeTargetFolder.Folder(), "IgnorePendingChangesAndDoNotPush", errorsAndInfos);
    }
}