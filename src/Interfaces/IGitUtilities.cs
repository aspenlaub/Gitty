using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using LibGit2Sharp;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface IGitUtilities {
        /// <summary>
        /// Return a folder where reusable downloads can be stored
        /// </summary>
        /// <returns></returns>
        string DownloadFolder();

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
        /// <param name="branch"></param>
        /// <param name="folder"></param>
        /// <param name="cloneOptions"></param>
        /// <param name="useCache"></param>
        /// <param name="errorsAndInfos"></param>
        void Clone(string url, string branch, IFolder folder, CloneOptions cloneOptions, bool useCache, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Cover of Repository.Clone
        /// </summary>
        /// <param name="url"></param>
        /// <param name="branch"></param>
        /// <param name="folder"></param>
        /// <param name="cloneOptions"></param>
        /// <param name="useCache"></param>
        /// <param name="extraCacheCondition"></param>
        /// <param name="onCloned"></param>
        /// <param name="errorsAndInfos"></param>
        void Clone(string url, string branch, IFolder folder, CloneOptions cloneOptions, bool useCache, Func<bool> extraCacheCondition, Action onCloned, IErrorsAndInfos errorsAndInfos);

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
        /// Reset repository to a specific head tip sha and remove untracked files
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="headTipIdSha"></param>
        /// <param name="errorsAndInfos"></param>
        void Reset(IFolder repositoryFolder, string headTipIdSha, IErrorsAndInfos errorsAndInfos);

        /// <summary>
        /// Return files with uncommitted changes
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <returns></returns>
        IList<string> FilesWithUncommittedChanges(IFolder repositoryFolder);

        /// <summary>
        /// Pull in latest changes
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <param name="author"></param>
        /// <param name="eMail"></param>
        void Pull(IFolder repositoryFolder, string author, string eMail);

        /// <summary>
        /// Return all shas
        /// </summary>
        /// <param name="repositoryFolder"></param>
        /// <returns></returns>
        IList<string> AllIdShas(IFolder repositoryFolder);
    }
}
