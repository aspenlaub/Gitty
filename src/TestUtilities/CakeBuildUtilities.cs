using System.IO;
using System.Text.RegularExpressions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class CakeBuildUtilities {
        public static void CopyLatestBuildCakeScript(ITestTargetFolder testTargetFolder, IErrorsAndInfos errorsAndInfos) {
            ILatestBuildCakeScriptProvider latestBuildCakeScriptProvider = new LatestBuildCakeScriptProvider();
            var latestBuildCakeScript = latestBuildCakeScriptProvider.GetLatestBuildCakeScript();
            if (latestBuildCakeScript.Length < 120 || !latestBuildCakeScript.Contains("#load \"solution.cake\"")) {
                errorsAndInfos.Errors.Add(Properties.Resources.CouldNotLoadLatestBuildCake);
                return;
            }

            var currentScriptFileName = testTargetFolder.FullName() + @"\" + "build.cake";
            if (File.Exists(currentScriptFileName)) {
                var currentScript = File.ReadAllText(currentScriptFileName);
                if (Regex.Replace(latestBuildCakeScript, @"\s", "") == Regex.Replace(currentScript, @"\s", "")) { return; }
            }

            File.WriteAllText(currentScriptFileName, latestBuildCakeScript);
        }
    }
}
