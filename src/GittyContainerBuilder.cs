using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

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

        // ReSharper disable once UnusedMember.Global
        public static IServiceCollection UseGitty(this IServiceCollection services) {
            services.AddTransient<ICakeInstaller, CakeInstaller>();
            services.AddTransient<ICakeRunner, CakeRunner>();
            services.AddTransient<IGitHubUtilities, GitHubUtilities>();
            services.AddTransient<IGitUtilities, GitUtilities>();
            services.AddTransient<IProcessRunner, ProcessRunner>();

            var componentProvider = new ComponentProvider();
            services.AddSingleton(componentProvider.SecretRepository);
            return services;
        }
    }
}
