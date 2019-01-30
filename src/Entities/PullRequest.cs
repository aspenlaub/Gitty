using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities {
    public class PullRequest : IPullRequest {
        public string Id { get; set; }
        public string Number { get; set; }
        public string State { get; set; }
        public string Branch { get; set; }
        public string Sha { get; set; }
    }
}
