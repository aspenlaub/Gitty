using System.Reflection;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface IEmbeddedCakeScriptReader {
        string ReadCakeScriptFromAssembly(Assembly assembly, string buildCakeName);
    }
}
