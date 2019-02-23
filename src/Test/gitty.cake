#addin nuget:?package=Cake.Git&version=0.19.0
#addin nuget:?package=System.Runtime.Loader&version=4.0.0.0
#addin nuget:https://www.aspenlaub.net/nuget/?package=Aspenlaub.Net.GitHub.CSharp.Gitty&loaddependencies=true&version=1.0.6993.22085

using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System.Runtime.Loader;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.TestUtilities;

var target = Argument("target", "Default");
var container = new ContainerBuilder().UseGittyTestUtilities().UseGitty().Build();

Task("Default")
  .Does(() => {
    var cakeInstaller = container.Resolve<ICakeInstaller>();
	if (cakeInstaller == null) {
	  throw new Exception("Could not resolve ICakeInstaller");
	}
	var cakeFolder = new Folder(@"C:\temp\" + System.Guid.NewGuid());
	cakeFolder.CreateIfNecessary();
	IErrorsAndInfos errorsAndInfos;
	cakeInstaller.InstallCake(cakeFolder, out errorsAndInfos);
	if (errorsAndInfos.AnyErrors()) {
	  throw new Exception(errorsAndInfos.ErrorsToString());
	}
	var deleter = new FolderDeleter();
	deleter.DeleteFolder(cakeFolder);
  });

  RunTarget(target);