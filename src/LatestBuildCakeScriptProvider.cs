using System.IO;
using System.Linq;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class LatestBuildCakeScriptProvider : ILatestBuildCakeScriptProvider {
        public string GetLatestBuildCakeScript(string buildCakeName) {
            var assembly = Assembly.GetExecutingAssembly();
            var names = assembly.GetManifestResourceNames();
            var name = names.FirstOrDefault(n => n.StartsWith("Aspenlaub.Net.GitHub.CSharp.") && n.EndsWith("." + buildCakeName));
            if (name == null) { return ""; }

            var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) { return ""; }

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
