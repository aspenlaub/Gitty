using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public static class GittyTestUtilitiesContainerBuilder {
        public static ContainerBuilder UseGittyTestUtilities(this ContainerBuilder builder) {
            builder.RegisterType<EmbeddedCakeScriptCopier>().As<IEmbeddedCakeScriptCopier>();
            builder.RegisterType<TestTargetRunner>().As<ITestTargetRunner>();
            return builder;
        }
    }
}
