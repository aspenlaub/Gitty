using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities {
    public static class GittyTestUtilitiesContainerBuilder {
        public static ContainerBuilder UseGittyTestUtilities(this ContainerBuilder builder) {
            builder.RegisterType<CakeBuildUtilities>();
            builder.RegisterType<TestTargetInstaller>();
            builder.RegisterType<TestTargetRunner>();
            return builder;
        }
    }
}
