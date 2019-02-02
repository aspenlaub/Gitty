using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public class CakeBuildUtilities {
        public void CopyCakeScriptEmbeddedInAssembly(Assembly assembly, string buildCakeName, ITestTargetFolder testTargetFolder, IErrorsAndInfos errorsAndInfos) {
            IEmbeddedCakeScriptReader embeddedCakeScriptReader = new EmbeddedCakeScriptReader();
            var embeddedCakeScript = embeddedCakeScriptReader.ReadCakeScriptFromAssembly(assembly, buildCakeName);
            if (embeddedCakeScript.Length < 120 || !embeddedCakeScript.Contains("#load \"solution.cake\"")) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotLoadEmbeddedBuildCake, buildCakeName, assembly.FullName));
                return;
            }

            var currentScriptFileName = testTargetFolder.FullName() + @"\" + buildCakeName;
            if (File.Exists(currentScriptFileName)) {
                var currentScript = File.ReadAllText(currentScriptFileName);
                if (Regex.Replace(embeddedCakeScript, @"\s", "") == Regex.Replace(currentScript, @"\s", "")) { return; }
            }

            File.WriteAllText(currentScriptFileName, embeddedCakeScript);
        }
    }
}
