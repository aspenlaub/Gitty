using System.IO;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

public class TestTargetRunner : ITestTargetRunner {
    private readonly IDotNetCakeRunner CakeRunner;
    private readonly IEmbeddedCakeScriptReader EmbeddedCakeScriptReader;

    public TestTargetRunner(IDotNetCakeRunner cakeRunner, IEmbeddedCakeScriptReader embeddedCakeScriptReader) {
        CakeRunner = cakeRunner;
        EmbeddedCakeScriptReader = embeddedCakeScriptReader;
    }

    public void RunBuildCakeScript(string buildCakeName, ITestTargetFolder testTargetFolder, string target, IErrorsAndInfos errorsAndInfos) {
        var scriptFileFullName = testTargetFolder.FullName() + @"\" + buildCakeName;
        CakeRunner.CallCake(scriptFileFullName, target, errorsAndInfos);
    }

    public void IgnoreOutdatedBuildCakePendingChangesAndDoNotPush(Assembly assembly, ITestTargetFolder targetFolder, IErrorsAndInfos errorsAndInfos) {
        var cakeScript = EmbeddedCakeScriptReader.ReadCakeScriptFromAssembly(assembly, BuildCake.Standard, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) { return; }

        var cakeScriptFileFullName = targetFolder.Folder().FullName + @"\" + BuildCake.Standard;
        File.WriteAllText(cakeScriptFileFullName, cakeScript);

        RunBuildCakeScript(BuildCake.Standard, targetFolder, "IgnoreOutdatedBuildCakePendingChangesAndDoNotPush", errorsAndInfos);
    }
}