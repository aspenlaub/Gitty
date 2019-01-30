using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using LibGit2Sharp;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface IGitUtilities {
        /// <summary>
        /// Given a git check-out folder, what is the friendly branch name?
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        string CheckedOutBranch(IFolder folder);

        /// <summary>
        /// Cover of Repository.Clone
        /// </summary>
        /// <param name="url"></param>
        /// <param name="folder"></param>
        /// <param name="cloneOptions"></param>
        /// <param name="useCache"></param>
        /// <param name="errorsAndInfos"></param>
        void Clone(string url, IFolder folder, CloneOptions cloneOptions, bool useCache, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Cover of Repository.Clone
        /// </summary>
        /// <param name="url"></param>
        /// <param name="folder"></param>
        /// <param name="cloneOptions"></param>
        /// <param name="useCache"></param>
        /// <param name="extraCacheCondition"></param>
        /// <param name="onCloned"></param>
        /// <param name="errorsAndInfos"></param>
        void Clone(string url, IFolder folder, CloneOptions cloneOptions, bool useCache, Func<bool> extraCacheCondition, Action onCloned, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Return the sha of the latest commit
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <returns></returns>
        string HeadTipIdSha(IFolder repositoryFolder);

        /// <summary>
        /// Verify that the local repository does not contain uncommitted changes
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="errorsAndInfos"></param>
        /// <returns></returns>
        void VerifyThatThereAreNoUncommittedChanges(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Check if checked out branch is ahead of the master
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <returns></returns>
        bool IsBranchAheadOfMaster(IFolder repositoryFolder);

        /// <summary>
        /// Find out owner and name from checked out repository
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="owner"></param>
        /// <param name="name"></param>
        /// <param name="errorsAndInfos"></param>
        void IdentifyOwnerAndName(IFolder repositoryFolder, out string owner, out string name, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Download and unpack PinnedCakeVersion (see CakeRunner) so that we do not need to install it using a powershell script
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="errorsAndInfos"></param>
        void DownloadReadyToCake(IFolder folder, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Reset repository to a specific head tip sha
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="headTipIdSha"></param>
        /// <param name="errorsAndInfos"></param>
        void Reset(IFolder repositoryFolder, string headTipIdSha, IErrorsAndInfos errorsAndInfos);
    }
}
