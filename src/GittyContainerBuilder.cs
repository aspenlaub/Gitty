using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.PeghStandard;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public static class GittyContainerBuilder {
        public static ContainerBuilder UseGittyAndPegh(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
            builder.UsePegh(csArgumentPrompter);
            builder.RegisterType<CakeInstaller>().As<ICakeInstaller>();
            builder.RegisterType<CakeRunner>().As<ICakeRunner>();
            builder.RegisterType<EmbeddedCakeScriptReader>().As<IEmbeddedCakeScriptReader>();
            builder.RegisterType<GitHubUtilities>().As<IGitHubUtilities>();
            builder.RegisterType<GitUtilities>().As<IGitUtilities>();
            builder.RegisterType<ProcessRunner>().As<IProcessRunner>();

            return builder;
        }

        // ReSharper disable once UnusedMember.Global
        public static IServiceCollection UseGittyAndPegh(this IServiceCollection services, ICsArgumentPrompter csArgumentPrompter) {
            services.UsePegh(csArgumentPrompter);
            services.AddTransient<ICakeInstaller, CakeInstaller>();
            services.AddTransient<ICakeRunner, CakeRunner>();
            services.AddTransient<IEmbeddedCakeScriptReader, EmbeddedCakeScriptReader>();
            services.AddTransient<IGitHubUtilities, GitHubUtilities>();
            services.AddTransient<IGitUtilities, GitUtilities>();
            services.AddTransient<IProcessRunner, ProcessRunner>();

            return services;
        }
    }
}
