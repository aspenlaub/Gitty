using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities {
    public class SimpleLoggingScopeId : ISimpleLoggingScopeId {
        public string Class { get; set; }
        public string Id { get; set; }

        public static ISimpleLoggingScopeId Create(string className, string id) {
            return new SimpleLoggingScopeId { Class = className, Id = id };
        }
    }
}
