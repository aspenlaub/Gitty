using System.Collections.Generic;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;

[XmlRoot("PersonalAccessTokens", Namespace = "http://www.aspenlaub.net")]
public class PersonalAccessTokens : List<PersonalAccessToken>, ISecretResult<PersonalAccessTokens> {
    public PersonalAccessTokens Clone() {
        var clone = new PersonalAccessTokens();
        clone.AddRange(this);
        return clone;
    }
}