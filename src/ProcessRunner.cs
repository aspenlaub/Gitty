using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class ProcessRunner : IProcessRunner {
        public void RunProcess(string executableFullName, string arguments, string workingFolder, IErrorsAndInfos errorsAndInfos) {
            using (var process = CreateProcess(executableFullName, arguments, workingFolder)) {
                var outputWaitHandle = new AutoResetEvent(false);
                var errorWaitHandle = new AutoResetEvent(false);
                process.OutputDataReceived += (sender, e) => {
                    OnDataReceived(e, outputWaitHandle, errorsAndInfos.Infos);
                };
                process.ErrorDataReceived += (sender, e) => {
                    OnDataReceived(e, errorWaitHandle, errorsAndInfos.Errors);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                outputWaitHandle.WaitOne();
                errorWaitHandle.WaitOne();
            }
        }

        private static void OnDataReceived(DataReceivedEventArgs e, EventWaitHandle waitHandle, ICollection<string> messages) {
            if (e.Data == null) {
                waitHandle.Set();
                return;
            }

            messages.Add(e.Data);
        }

        private static Process CreateProcess(string executableFullName, string arguments, string workingFolder) {
            return new Process {
                StartInfo = {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = executableFullName,
                    Arguments = arguments,
                    WorkingDirectory = workingFolder,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
        }
    }
}
