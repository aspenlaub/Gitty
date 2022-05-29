using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;

public class PersonalAccessTokensSecret : ISecret<PersonalAccessTokens> {
    private PersonalAccessTokens DefaultPersonalAccessTokens;
    public PersonalAccessTokens DefaultValue => DefaultPersonalAccessTokens ??= new PersonalAccessTokens();

    public string Guid => "D72CD90D-EA45-430B-96E7-3AF71EED408B";
}