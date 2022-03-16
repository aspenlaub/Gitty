using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components {
    public class GitHubUtilities : IGitHubUtilities {
        private readonly IGitUtilities GitUtilities;
        private readonly ISecretRepository SecretRepository;

        public GitHubUtilities(IGitUtilities gitUtilities, ISecretRepository secretRepository) {
            GitUtilities = gitUtilities;
            SecretRepository = secretRepository;
        }

        public async Task<bool> HasOpenPullRequestAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            var pullRequests = await GetPullRequestsAsync(repositoryFolder, "open", errorsAndInfos);
            return pullRequests.Any();
        }

        public async Task<bool> HasOpenPullRequestAsync(IFolder repositoryFolder, string semicolonSeparatedListOfPullRequestNumbersToIgnore, IErrorsAndInfos errorsAndInfos) {
            var pullRequests = await GetPullRequestsAsync(repositoryFolder, "open", errorsAndInfos);
            return pullRequests.Any(p => !semicolonSeparatedListOfPullRequestNumbersToIgnore.Split(';').Contains(p.Number));
        }

        public async Task<bool> HasOpenPullRequestForThisBranchAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            var pullRequests = await GetPullRequestsAsync(repositoryFolder, "open", errorsAndInfos);
            var checkedOutBranch = GitUtilities.CheckedOutBranch(repositoryFolder);
            return pullRequests.Any(p => p.Branch == checkedOutBranch);
        }

        public async Task<int> GetNumberOfPullRequestsAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            return (await GetPullRequestsAsync(repositoryFolder, "all", errorsAndInfos)).Count;
        }


        public async Task<bool> HasPullRequestForThisBranchAndItsHeadTipAsync(IFolder repositoryFolder, IErrorsAndInfos errorsAndInfos) {
            var pullRequests = await GetPullRequestsAsync(repositoryFolder, "all", errorsAndInfos);
            var checkedOutBranch = GitUtilities.CheckedOutBranch(repositoryFolder);
            var headTipIdSha = GitUtilities.HeadTipIdSha(repositoryFolder);
            return pullRequests.Any(p => p.Branch == checkedOutBranch && p.Sha == headTipIdSha);
        }

        protected async Task<IList<IPullRequest>> GetPullRequestsAsync(IFolder repositoryFolder, string state, IErrorsAndInfos errorsAndInfos) {
            var pullRequests = new List<IPullRequest>();
            GitUtilities.IdentifyOwnerAndName(repositoryFolder, out var owner, out var name, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return pullRequests; }


            var url = $"https://api.github.com/repos/{owner}/{name}/pulls?state=" + state;
            var result = await RunJsonWebRequestAsync(url, owner, errorsAndInfos) as JArray;
            if (errorsAndInfos.AnyErrors()) { return pullRequests; }

            if (result == null) {
                errorsAndInfos.Errors.Add(Properties.Resources.CouldNotGetListOfPullRequests);
                return pullRequests;
            }

            pullRequests.AddRange(result.Select(detailResult => CreatePullRequest(detailResult)));

            return pullRequests;
        }

        protected async Task<object> RunJsonWebRequestAsync(string url, string owner, IErrorsAndInfos errorsAndInfos) {
            var personalAccessTokens = await GetPersonalAccessTokensAsync(errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) {
                return null;
            }
            var personalAccessToken = personalAccessTokens.FirstOrDefault(p => p.Owner == owner && p.Purpose == "API");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", GetType().Namespace);
            if (personalAccessToken != null) {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken.Token);
            }
            var response = await client.GetAsync(url);
            if (HttpStatusCode.OK != response.StatusCode) {
                errorsAndInfos.Errors.Add(Properties.Resources.CouldNotGetListOfPullRequests);
                return null;
            }

            string text;
            try {
                text = await response.Content.ReadAsStringAsync();
            } catch {
                errorsAndInfos.Errors.Add(Properties.Resources.CouldNotGetListOfPullRequests);
                return null;
            }
            return JsonConvert.DeserializeObject(text);
        }

        protected static PullRequest CreatePullRequest(JToken jToken) {
            return new() {
                Id = jToken["id"].Value<string>(),
                Number = jToken["number"].Value<string>(),
                State = jToken["state"].Value<string>(),
                Branch = (jToken["head"]["ref"] ?? "").Value<string>(),
                Sha = (jToken["head"]["sha"] ?? "").Value<string>()
            };
        }

        private async Task<PersonalAccessTokens> GetPersonalAccessTokensAsync(IErrorsAndInfos errorsAndInfos) {
            var personalAccessTokensSecret = new PersonalAccessTokensSecret();
            var personalAccessTokens = await SecretRepository.GetAsync(personalAccessTokensSecret, errorsAndInfos);
            return personalAccessTokens;
        }
    }

}
