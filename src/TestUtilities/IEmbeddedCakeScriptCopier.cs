using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities.Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public interface IEmbeddedCakeScriptCopier {
        void CopyCakeScriptEmbeddedInAssembly(Assembly assembly, string buildCakeName, ITestTargetFolder testTargetFolder, IErrorsAndInfos errorsAndInfos);
    }
}