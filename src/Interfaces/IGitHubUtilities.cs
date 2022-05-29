using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

public interface IGitHubUtilities {
    Task<bool> HasOpenPullRequestAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos);
    Task<bool> HasOpenPullRequestAsync(IFolder repositoryFolder, string semicolonSeparatedListOfPullRequestNumbersToIgnore, IErrorsAndInfos errorsAndInfos);
    Task<bool> HasOpenPullRequestForThisBranchAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos);
    Task<int> GetNumberOfPullRequestsAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos);
    Task<bool> HasPullRequestForThisBranchAndItsHeadTipAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos);
}