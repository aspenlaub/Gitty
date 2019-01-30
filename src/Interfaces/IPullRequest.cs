// ReSharper disable UnusedMember.Global
namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface IPullRequest {
        string Id { get; }
        string Number { get; }
        string State { get; }
        string Branch { get; }
        string Sha { get; }
    }
}
