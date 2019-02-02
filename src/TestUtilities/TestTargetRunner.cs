using System.IO;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class TestTargetRunner {
        private readonly ICakeRunner vCakeRunner;

        public TestTargetRunner(ICakeRunner cakeRunner) {
            vCakeRunner = cakeRunner;
        }

        public void RunBuildCakeScript(string buildCakeName, ITestTargetFolder testTargetFolder, string target, IErrorsAndInfos errorsAndInfos) {
            var cakeExeFileFullName = testTargetFolder.CakeFolder().SubFolder("tools").SubFolder("Cake").FullName + @"\cake.exe";
            if (!File.Exists(cakeExeFileFullName)) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FileNotFound, cakeExeFileFullName));
                return;
            }

            var scriptFileFullName = testTargetFolder.FullName() + @"\" + buildCakeName;
            vCakeRunner.CallCake(cakeExeFileFullName, scriptFileFullName, target, errorsAndInfos);
        }

        public void IgnoreOutdatedBuildCakePendingChangesAndDoNotPush(Assembly assembly, ITestTargetFolder targetFolder, IErrorsAndInfos errorsAndInfos) {
            var embeddedCakeScriptReader = new EmbeddedCakeScriptReader();
            var cakeScript = embeddedCakeScriptReader.ReadCakeScriptFromAssembly(assembly, BuildCake.Standard);
            var cakeScriptFileFullName = targetFolder.Folder().FullName + @"\" + BuildCake.Standard;
            File.WriteAllText(cakeScriptFileFullName, cakeScript);

            RunBuildCakeScript(BuildCake.Standard, targetFolder, "IgnoreOutdatedBuildCakePendingChangesAndDoNotPush", errorsAndInfos);
        }
    }
}
