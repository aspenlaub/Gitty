using System.IO;
using System.Linq;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class EmbeddedCakeScriptReader : IEmbeddedCakeScriptReader {
    public string ReadCakeScriptFromAssembly(Assembly assembly, string buildCakeName, IErrorsAndInfos errorsAndInfos) {
        var names = assembly.GetManifestResourceNames();
        var name = names.FirstOrDefault(n => n.StartsWith("Aspenlaub.Net.GitHub.CSharp.") && n.EndsWith("." + buildCakeName));
        if (name == null) {
            errorsAndInfos.Errors.Add(Properties.Resources.ResourceNotFound);
            return "";
        }

        var stream = assembly.GetManifestResourceStream(name);
        if (stream == null) {
            errorsAndInfos.Errors.Add(Properties.Resources.ResourceNotFound);
            return "";
        }

        var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}