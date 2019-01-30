using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities {
    public class PersonalAccessToken {
        [XmlAttribute("owner")]
        public string Owner { get; set; }

        [XmlAttribute("name")]
        public string TokenName { get; set; }

        [XmlAttribute("token")]
        public string Token { get; set; }
    }
}
