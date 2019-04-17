using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class ProcessRunner : IProcessRunner {
        private readonly ISimpleLogger vSimpleLogger;

        public ProcessRunner(ISimpleLogger simpleLogger) {
            vSimpleLogger = simpleLogger;
        }

        public void RunProcess(string executableFullName, string arguments, string workingFolder, IErrorsAndInfos errorsAndInfos) {
            var id = Guid.NewGuid().ToString();
            using (vSimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ProcessRunner), id))) {
                vSimpleLogger.LogInformation($"Running {executableFullName} with arguments {arguments} in {workingFolder}");
                using (var process = CreateProcess(executableFullName, arguments, workingFolder)) {
                    try {
                        var outputWaitHandle = new AutoResetEvent(false);
                        var errorWaitHandle = new AutoResetEvent(false);
                        process.OutputDataReceived += (sender, e) => { OnDataReceived(e, outputWaitHandle, errorsAndInfos.Infos, LogLevel.Information); };
                        process.ErrorDataReceived += (sender, e) => { OnDataReceived(e, errorWaitHandle, errorsAndInfos.Errors, LogLevel.Error); };
                        process.Exited += (sender, e) => { vSimpleLogger.LogInformation("Process exited"); };
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                        outputWaitHandle.WaitOne();
                        errorWaitHandle.WaitOne();
                    } catch (Exception e) {
                        errorsAndInfos.Errors.Add($"Process failed: {e.Message}");
                        return;
                    }
                }

                vSimpleLogger.LogInformation("Process completed");
            }
        }

        private void OnDataReceived(DataReceivedEventArgs e, EventWaitHandle waitHandle, ICollection<string> messages, LogLevel logLevel) {
            if (e.Data == null) {
                waitHandle.Set();
                return;
            }

            messages.Add(e.Data);
            vSimpleLogger.Log(logLevel, e.Data);
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
