using System.IO;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class TestTargetRunner {
        public void RunBuildCakeScript(ITestTargetFolder testTargetFolder, ICakeRunner cakeRunner, string target, IErrorsAndInfos errorsAndInfos) {
            var cakeExeFileFullName = testTargetFolder.CakeFolder().SubFolder("tools").SubFolder("Cake").FullName + @"\cake.exe";
            if (!File.Exists(cakeExeFileFullName)) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FileNotFound, cakeExeFileFullName));
                return;
            }

            var scriptFileFullName = testTargetFolder.FullName() + @"\" + "build.cake";
            cakeRunner.CallCake(cakeExeFileFullName, scriptFileFullName, target, errorsAndInfos);
        }

    }
}
