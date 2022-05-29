﻿using Aspenlaub.Net.GitHub.CSharp.Gitty.Components;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty;

public static class GittyContainerBuilder {
    public static ContainerBuilder UseGittyAndPegh(this ContainerBuilder builder, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        builder.UsePegh(applicationName, csArgumentPrompter);
        builder.RegisterType<DotNetCakeInstaller>().As<IDotNetCakeInstaller>();
        builder.RegisterType<ProcessRunner>().As<IProcessRunner>();
        builder.RegisterType<EmbeddedCakeScriptReader>().As<IEmbeddedCakeScriptReader>();
        builder.RegisterType<DotNetCakeRunner>().As<IDotNetCakeRunner>();
        builder.RegisterType<GitUtilities>().As<IGitUtilities>();
        builder.RegisterType<GitHubUtilities>().As<IGitHubUtilities>();

        return builder;
    }

    // ReSharper disable once UnusedMember.Global
    public static IServiceCollection UseGittyAndPegh(this IServiceCollection services, string applicationName, ICsArgumentPrompter csArgumentPrompter) {
        services.UsePegh(applicationName, csArgumentPrompter);
        services.AddTransient<IDotNetCakeInstaller, DotNetCakeInstaller>();
        services.AddTransient<IProcessRunner, ProcessRunner>();
        services.AddTransient<IEmbeddedCakeScriptReader, EmbeddedCakeScriptReader>();
        services.AddTransient<IDotNetCakeRunner, DotNetCakeRunner>();
        services.AddTransient<IGitUtilities, GitUtilities>();
        services.AddTransient<IGitHubUtilities, GitHubUtilities>();

        return services;
    }
}