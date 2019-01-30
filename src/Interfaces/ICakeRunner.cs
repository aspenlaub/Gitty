using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ICakeRunner {
        /// <summary>
        /// Call cake and run build.cake in a specified folder, return errors
        /// </summary>
        /// <param name="cakeExeFullName"></param>
        /// <param name="scriptFileFullName"></param>
        /// <param name="errorsAndInfos"></param>
        void CallCake(string cakeExeFullName, string scriptFileFullName, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Call cake and run build.cake in a specified folder, return errors
        /// </summary>
        /// <param name="cakeExeFullName"></param>
        /// <param name="scriptFileFullName"></param>
        /// <param name="target"></param>
        /// <param name="errorsAndInfos"></param>
        void CallCake(string cakeExeFullName, string scriptFileFullName, string target, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Verify that we are using the pinned cake version
        /// </summary>
        /// <param name="toolsFolder"></param>
        /// <param name="errorsAndInfos"></param>
        void VerifyCakeVersion(IFolder toolsFolder, IErrorsAndInfos errorsAndInfos);
    }
}
