using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;

public class BranchWithPackages {
    [Key, XmlAttribute("branch")]
    public string Branch { get; set; }
}