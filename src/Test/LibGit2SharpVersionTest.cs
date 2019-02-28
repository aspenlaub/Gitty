using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable LoopCanBePartlyConvertedToQuery

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Test {
    [TestClass]
    public class LibGit2SharpVersionTest {
        [TestMethod]
        public void NativeBinariesIdenticalAcrossProjects() {
            var location = typeof(LibGit2SharpVersionTest).Assembly.Location;
            Assert.IsFalse(string.IsNullOrWhiteSpace(location), "Could not locate test assembly");
            Assert.IsTrue(location.Contains(@"\src\"), "Test assembly is supposed to have src as a parent folder");
            location = location.Substring(0, location.IndexOf(@"\src\", StringComparison.InvariantCultureIgnoreCase) + 5);
            string git2Native = "", file ="";
            foreach (var otherFile in Directory.GetFiles(location, "*.deps.json", SearchOption.AllDirectories).Where(f => f.Contains("Debug"))) {
                var lines = File.ReadAllLines(otherFile).Where(l => l.Contains(@"/git2-") && l.Contains(".dll")).ToList();
                foreach (var line in lines) {
                    var otherGit2Native = line.Substring(line.IndexOf("git2-", StringComparison.InvariantCultureIgnoreCase));
                    otherGit2Native = otherGit2Native.Substring(0, otherGit2Native.IndexOf(".dll", StringComparison.InvariantCultureIgnoreCase) + 4);
                    if (git2Native == "") {
                        file = otherFile;
                        git2Native = otherGit2Native;
                        continue;
                    }
                    Assert.IsTrue(git2Native == otherGit2Native, $"Two different LibGit2Sharp native assemblies found: {git2Native} ({file}) and {otherGit2Native} ({otherFile})");
                }
            }
        }
    }
}
