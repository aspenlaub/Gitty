#addin nuget:?package=System.Runtime.Loader&version=4.0.0.0
#addin nuget:https://www.aspenlaub.net/nuget/?package=Aspenlaub.Net.GitHub.CSharp.Gitty&loaddependencies=true&version=1.0.6998.26503

using Microsoft.Extensions.DependencyInjection;
using Autofac;
using System.IO;
using System.Runtime.Loader;
using LibGit2Sharp;
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
    var aspenlaubTempFolder = new Folder(System.IO.Path.GetTempPath()).SubFolder("AspenlaubTemp");
    aspenlaubTempFolder.CreateIfNecessary();
    var cakeInstaller = container.Resolve<ICakeInstaller>();
	  if (cakeInstaller == null) {
	    throw new Exception("Could not resolve ICakeInstaller");
	  }
	  var cakeFolder = aspenlaubTempFolder.SubFolder(System.Guid.NewGuid().ToString());
	  cakeFolder.CreateIfNecessary();
	  IErrorsAndInfos errorsAndInfos;
	  cakeInstaller.InstallCake(cakeFolder, out errorsAndInfos);
	  if (errorsAndInfos.AnyErrors()) {
	    throw new Exception(errorsAndInfos.ErrorsToString());
	  }
    var pakledFolder = aspenlaubTempFolder.SubFolder(System.Guid.NewGuid().ToString());
    const string url = "https://github.com/aspenlaub/PakledCore.git";
    Repository.Clone(url, pakledFolder.FullName, new CloneOptions { BranchName = "master" });
	  var deleter = new FolderDeleter();
	  deleter.DeleteFolder(cakeFolder);
	  deleter.DeleteFolder(pakledFolder);
  });

  RunTarget(target);