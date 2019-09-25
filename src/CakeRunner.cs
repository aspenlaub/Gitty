using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class CakeRunner : ICakeRunner {
        public const string PinnedCakeVersion = "0.34.1", PreviousPinnedCakeVersion = "0.34.1";

        private readonly IProcessRunner vProcessRunner;

        public CakeRunner(IProcessRunner processRunner) {
            vProcessRunner = processRunner;
        }

        public void CallCake(string cakeExeFullName, string scriptFileFullName, IErrorsAndInfos errorsAndInfos) {
            CallCake(cakeExeFullName, scriptFileFullName, "", errorsAndInfos);
        }

        public void CallCake(string cakeExeFullName, string scriptFileFullName, string target, IErrorsAndInfos errorsAndInfos) {
            if (!File.Exists(cakeExeFullName)) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FileNotFound, cakeExeFullName));
                return;
            }

            if (!File.Exists(scriptFileFullName)) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.FileNotFound, scriptFileFullName));
                return;
            }

            var toolsFolder = new Folder(cakeExeFullName.Substring(0, 6 + cakeExeFullName.LastIndexOf(@"\" + @"tools\", StringComparison.Ordinal)));
            VerifyCakeVersion(toolsFolder, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) { return; }

            var scriptFileFolderFullName = scriptFileFullName.Substring(0, scriptFileFullName.LastIndexOf('\\'));
            var arguments = "\"" + scriptFileFullName + "\" -mono";
            if (target != "") {
                arguments = arguments + " -target=" + target;
            }
            vProcessRunner.RunProcess(cakeExeFullName, arguments, scriptFileFolderFullName, errorsAndInfos);
        }

        public void VerifyCakeVersion(IFolder toolsFolder, IErrorsAndInfos errorsAndInfos) {
            var packagesConfigFileFullName = toolsFolder.FullName + @"\packages.config";
            var document = XDocument.Load(packagesConfigFileFullName);
            var element = document.XPathSelectElements("/packages/package").FirstOrDefault(e => e.Attribute("id")?.Value == "Cake");
            if (element == null) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotReadCakeVersion, packagesConfigFileFullName));
                return;
            }

            var attribute = element.Attribute("version");
            if (attribute == null) {
                errorsAndInfos.Errors.Add(string.Format(Properties.Resources.CouldNotReadCakeVersion, packagesConfigFileFullName));
                return;
            }

            if (attribute.Value == PinnedCakeVersion || attribute.Value == PreviousPinnedCakeVersion) { return; }

            errorsAndInfos.Errors.Add(string.Format(Properties.Resources.WrongCakeVersion, attribute.Value, packagesConfigFileFullName));
        }
    }
}
