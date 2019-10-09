using Aspenlaub.Net.GitHub.CSharp.Dvin.Components;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public static class GittyContainerBuilder {
        private static readonly ISimpleLogger SimpleLogger = new SimpleLogger(new SimpleLogFlusher());

        public static ContainerBuilder UseGitty(this ContainerBuilder builder, ICsArgumentPrompter csArgumentPrompter) {
            builder.RegisterForPegh(csArgumentPrompter);
            builder.RegisterType<CakeInstaller>().As<ICakeInstaller>();
            builder.RegisterType<CakeRunner>().As<ICakeRunner>();
            builder.RegisterType<EmbeddedCakeScriptReader>().As<IEmbeddedCakeScriptReader>();
            builder.RegisterType<GitHubUtilities>().As<IGitHubUtilities>();
            builder.RegisterType<GitUtilities>().As<IGitUtilities>();
            builder.RegisterType<ProcessRunner>().As<IProcessRunner>();
            builder.RegisterInstance(SimpleLogger);
            builder.RegisterInstance<ILogger>(SimpleLogger);

            return builder;
        }

        // ReSharper disable once UnusedMember.Global
        public static IServiceCollection UseGitty(this IServiceCollection services, ICsArgumentPrompter csArgumentPrompter) {
            services.UseDvinAndPegh(csArgumentPrompter);
            services.AddTransient<ICakeInstaller, CakeInstaller>();
            services.AddTransient<ICakeRunner, CakeRunner>();
            services.AddTransient<IEmbeddedCakeScriptReader, EmbeddedCakeScriptReader>();
            services.AddTransient<IGitHubUtilities, GitHubUtilities>();
            services.AddTransient<IGitUtilities, GitUtilities>();
            services.AddTransient<IProcessRunner, ProcessRunner>();
            services.AddSingleton(SimpleLogger);
            services.AddSingleton<ILogger>(SimpleLogger);

            return services;
        }
    }
}
