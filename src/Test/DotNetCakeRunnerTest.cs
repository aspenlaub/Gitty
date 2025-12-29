using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test;

[TestClass]
public class DotNetCakeRunnerTest {
    protected static IDotNetCakeRunner Sut;
    protected static IFolder ScriptsFolder;
    protected const string ThisIsNotCake = @"This is not a cake!";
    private static IContainer _container;

    [ClassInitialize]
    public static void Initialize(TestContext context) {
        _container = new ContainerBuilder().UseGittyAndPegh("Gitty", new DummyCsArgumentPrompter()).Build();

        ScriptsFolder = CakeScriptsFolder();
        DeleteFolder(ScriptsFolder);
        Directory.CreateDirectory(ScriptsFolder.FullName);

        IEmbeddedCakeScriptReader cakeScriptReader = _container.Resolve<IEmbeddedCakeScriptReader>();
        var errorsAndInfos = new ErrorsAndInfos();
        foreach (string cakeId in new[] { "success", "failure", "net5" }) {
            File.WriteAllText(ScriptsFolder.FullName + @"\" + cakeId + ".cake", cakeScriptReader.ReadCakeScriptFromAssembly(Assembly.GetExecutingAssembly(), cakeId + ".cake", errorsAndInfos));
            Assert.IsFalse(errorsAndInfos.AnyErrors(), errorsAndInfos.ErrorsToString());
        }

        Sut = _container.Resolve<IDotNetCakeRunner>();
    }

    [ClassCleanup]
    public static void Cleanup() {
        DeleteFolder(CakeScriptsFolder());
    }

    protected static IFolder CakeScriptsFolder() {
        return new Folder(Path.GetTempPath()).SubFolder("AspenlaubTemp").SubFolder(nameof(DotNetCakeRunnerTest));
    }

    private static void DeleteFolder(IFolder folder) {
        if (!folder.Exists()) {
            return;
        }

        var deleter = new FolderDeleter();
        deleter.DeleteFolder(folder);
    }

    [TestMethod]
    public void CanCallScriptWithoutErrors() {
        var errorsAndInfos = new ErrorsAndInfos();

        Sut.CallCake(ScriptsFolder.FullName + @"\success.cake", true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"Task")));
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"Duration")));
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"00:00:00")));
    }

    [TestMethod]
    public void CanCallScriptWithErrors() {
        var errorsAndInfos = new ErrorsAndInfos();

        Sut.CallCake(ScriptsFolder.FullName + @"\" + "failure.cake", true, errorsAndInfos);
        Assert.HasCount(1, errorsAndInfos.Errors);
        Assert.AreEqual("This is not a cake!", errorsAndInfos.Errors[0]);
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"Task")));
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"Duration")));
        Assert.IsTrue(errorsAndInfos.Infos.Any(m => m.Contains(@"00:00:00")));
        Assert.IsFalse(errorsAndInfos.Infos.Any(m => m.Contains(ThisIsNotCake)));
        ISimpleLogger logger = _container.Resolve<ISimpleLogger>();
        Assert.IsNotNull(logger);
        IList<ISimpleLogEntry> logEntries = logger.FindLogEntries(_ => true);
        Assert.IsTrue(errorsAndInfos.Errors.All(e => logEntries.Any(le => le.LogLevel == LogLevel.Error && le.Message.Contains(e))));
        Assert.IsTrue(errorsAndInfos.Infos.All(i => logEntries.Any(le => le.LogLevel == LogLevel.Information && le.Message.Contains(i))));
    }

    [TestMethod]
    public void CanCallScriptAgainstNonExistingTargetButGetAnError() {
        var errorsAndInfos = new ErrorsAndInfos();
        Sut.CallCake(ScriptsFolder.FullName + @"\success.cake", "NonExistingTarget", true, errorsAndInfos);
        Assert.IsTrue(errorsAndInfos.Errors.Any());
        Assert.IsTrue(errorsAndInfos.Errors.Any(e => e.Contains("The target 'NonExistingTarget' was not found")));
    }

    [TestMethod]
    public void CanCallScriptAgainstAlternativeTarget() {
        var errorsAndInfos = new ErrorsAndInfos();
        Sut.CallCake(ScriptsFolder.FullName + @"\success.cake", "AlternativeTarget", true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());
        Assert.IsTrue(errorsAndInfos.Infos.Any(i => i.Contains("This is an alternative target")));
    }

    [TestMethod]
    public void CanCallScriptWithNet5Addin() {
        var errorsAndInfos = new ErrorsAndInfos();

        Sut.CallCake(ScriptsFolder.FullName + @"\net5.cake", true, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.Errors.Any(), errorsAndInfos.ErrorsPlusRelevantInfos());
        const string constructionStatement = "Constructing a strong thing";
        Assert.Contains(constructionStatement, errorsAndInfos.Infos);
        Assert.AreEqual("Success", errorsAndInfos.Infos[errorsAndInfos.Infos.IndexOf(constructionStatement) + 1]);
    }
}