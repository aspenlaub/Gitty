using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test {
    [TestClass]
    public class SimpleLoggerTest {
        private const string NotAMessage = "This is not a message";

        [TestMethod]
        public void CanUseLogger() {
            var flusher = new SimpleLogFlusher();
            ISimpleLogger sut = new SimpleLogger(flusher);
            using (sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "A"))) {
                using (sut.BeginScope(SimpleLoggingScopeId.Create("Scope", "B"))) {
                    sut.Log(LogLevel.Information, new EventId(0), new Dictionary<string, object>(), null, (state, exception) => { return NotAMessage; } );
                }
            }

            var logEntries = sut.FindLogEntries(e => true);
            Assert.AreEqual(1, logEntries.Count);
            Assert.AreEqual(LogLevel.Information, logEntries[0].LogLevel);
            Assert.AreEqual(2, logEntries[0].Stack.Count);
            Assert.AreEqual("Scope(A)", logEntries[0].Stack[0]);
            Assert.AreEqual("Scope(B)", logEntries[0].Stack[1]);
            Assert.AreEqual(NotAMessage, logEntries[0].Message);

            var fileNames = flusher.FileNames;
            Assert.AreEqual(1, fileNames.Count);
            var fileName = fileNames.First();
            Assert.IsTrue(File.Exists(fileName));
            Assert.IsTrue(fileName.EndsWith(@"\Scope(A).log"));
            Assert.AreEqual(0, logEntries.Count(e => !e.Flushed));

            File.SetLastWriteTime(fileName, DateTime.Now.AddHours(-25));
            flusher.ResetCleanupTime();
            flusher.Flush(sut);
            Assert.IsFalse(File.Exists(fileName));
        }

        [TestMethod]
        public void CanResolveInstance() {
            var container = new ContainerBuilder().UseGittyDvinAndPegh(new DummyCsArgumentPrompter()).Build();
            var logger = container.Resolve<ILogger>();
            Assert.IsNotNull(logger);
            Assert.IsTrue(logger is SimpleLogger);

            var simpleLogger = container.Resolve<ISimpleLogger>();
            Assert.IsNotNull(simpleLogger);
        }
    }
}
