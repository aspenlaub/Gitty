using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

public class EmbeddedCakeScriptCopier(IEmbeddedCakeScriptReader embeddedCakeScriptReader) : IEmbeddedCakeScriptCopier {
    public void CopyCakeScriptEmbeddedInAssembly(Assembly assembly, string buildCakeName, ITestTargetFolder testTargetFolder, IErrorsAndInfos errorsAndInfos) {
        string embeddedCakeScript = embeddedCakeScriptReader.ReadCakeScriptFromAssembly(assembly, buildCakeName, errorsAndInfos);
        if (errorsAndInfos.AnyErrors()) {
            return;
        }
        if (embeddedCakeScript.Length < 120 || !embeddedCakeScript.Contains("#load \"solution.cake\"")) {
            errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotLoadEmbeddedBuildCake, buildCakeName, assembly.FullName));
            return;
        }

        string currentScriptFileName = testTargetFolder.FullName() + @"\" + buildCakeName;
        if (File.Exists(currentScriptFileName)) {
            string currentScript = File.ReadAllText(currentScriptFileName);
            if (Regex.Replace(embeddedCakeScript, @"\s", "") == Regex.Replace(currentScript, @"\s", "")) { return; }
        }

        File.WriteAllText(currentScriptFileName, embeddedCakeScript);
    }
}