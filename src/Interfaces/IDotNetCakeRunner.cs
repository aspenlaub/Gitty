using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface IDotNetCakeRunner {
        /// <summary>
        /// Run cake script in a specified folder, return errors
        /// </summary>
        /// <param name="scriptFileFullName"></param>
        /// <param name="errorsAndInfos"></param>
        void CallCake(string scriptFileFullName, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Run cake script with a specific target in a specified folder, return errors
        /// </summary>
        /// <param name="scriptFileFullName"></param>
        /// <param name="target"></param>
        /// <param name="errorsAndInfos"></param>
        void CallCake(string scriptFileFullName, string target, IErrorsAndInfos errorsAndInfos);
    }
}
