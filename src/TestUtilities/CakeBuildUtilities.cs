using System.IO;
using System.Text.RegularExpressions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class CakeBuildUtilities {
        public void CopyLatestBuildCakeScript(string buildCakeName, ITestTargetFolder testTargetFolder, IErrorsAndInfos errorsAndInfos) {
            ILatestBuildCakeScriptProvider latestBuildCakeScriptProvider = new LatestBuildCakeScriptProvider();
            var latestBuildCakeScript = latestBuildCakeScriptProvider.GetLatestBuildCakeScript(buildCakeName);
            if (latestBuildCakeScript.Length < 120 || !latestBuildCakeScript.Contains("#load \"solution.cake\"")) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotLoadLatestBuildCake, buildCakeName));
                return;
            }

            var currentScriptFileName = testTargetFolder.FullName() + @"\" + buildCakeName;
            if (File.Exists(currentScriptFileName)) {
                var currentScript = File.ReadAllText(currentScriptFileName);
                if (Regex.Replace(latestBuildCakeScript, @"\s", "") == Regex.Replace(currentScript, @"\s", "")) { return; }
            }

            File.WriteAllText(currentScriptFileName, latestBuildCakeScript);
        }
    }
}
