using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

public interface IEmbeddedCakeScriptReader {
    string ReadCakeScriptFromAssembly(Assembly assembly, string buildCakeName, IErrorsAndInfos errorsAndInfos);
}