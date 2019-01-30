using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public static class GittyContainerBuilder {
        public static ContainerBuilder UseGitty(this ContainerBuilder builder) {
            builder.RegisterType<CakeInstaller>().As<ICakeInstaller>();
            builder.RegisterType<CakeRunner>().As<ICakeRunner>();
            builder.RegisterType<GitHubUtilities>().As<IGitHubUtilities>();
            builder.RegisterType<GitUtilities>().As<IGitUtilities>();
            builder.RegisterType<ProcessRunner>().As<IProcessRunner>();

            var componentProvider = new ComponentProvider();
            builder.RegisterInstance(componentProvider.SecretRepository);
            return builder;
        }
    }
}
