using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class ProcessRunner : IProcessRunner {
    private readonly ISimpleLogger SimpleLogger;
    private readonly IMethodNamesFromStackFramesExtractor MethodNamesFromStackFramesExtractor;

    public ProcessRunner(ISimpleLogger simpleLogger, IMethodNamesFromStackFramesExtractor methodNamesFromStackFramesExtractor) {
        SimpleLogger = simpleLogger;
        MethodNamesFromStackFramesExtractor = methodNamesFromStackFramesExtractor;
    }

    public void RunProcess(string executableFileName, string arguments, IFolder workingFolder, IErrorsAndInfos errorsAndInfos) {
        var id = Guid.NewGuid().ToString();
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(ProcessRunner), id))) {
            var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            SimpleLogger.LogInformationWithCallStack($"Running {executableFileName} with arguments {arguments} in {workingFolder.FullName}", methodNamesFromStack);
            using (var process = CreateProcess(executableFileName, arguments, workingFolder)) {
                try {
                    var outputWaitHandle = new AutoResetEvent(false);
                    var errorWaitHandle = new AutoResetEvent(false);
                    process.OutputDataReceived += (_, e) => { OnDataReceived(e, outputWaitHandle, errorsAndInfos.Infos, LogLevel.Information); };
                    process.ErrorDataReceived += (_, e) => { OnDataReceived(e, errorWaitHandle, errorsAndInfos.Errors, LogLevel.Error); };
                    process.Exited += (_, _) => { SimpleLogger.LogInformationWithCallStack("Process exited", methodNamesFromStack); };
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit(int.MaxValue);
                    outputWaitHandle.WaitOne();
                    errorWaitHandle.WaitOne();
                } catch (Exception e) {
                    errorsAndInfos.Errors.Add($"Process failed: {e.Message}");
                    return;
                }
            }

            SimpleLogger.LogInformationWithCallStack("Process completed", methodNamesFromStack);
        }
    }

    private void OnDataReceived(DataReceivedEventArgs e, EventWaitHandle waitHandle, ICollection<string> messages, LogLevel logLevel) {
        if (e.Data == null) {
            waitHandle.Set();
            return;
        }

        messages.Add(e.Data);
        var id = Guid.NewGuid().ToString();
        using (SimpleLogger.BeginScope(SimpleLoggingScopeId.Create(nameof(OnDataReceived), id))) {
            var methodNamesFromStack = MethodNamesFromStackFramesExtractor.ExtractMethodNamesFromStackFrames();
            switch (logLevel) {
                case LogLevel.Warning:
                    SimpleLogger.LogWarningWithCallStack(e.Data, methodNamesFromStack);
                    break;
                case LogLevel.Error:
                    SimpleLogger.LogErrorWithCallStack(e.Data, methodNamesFromStack);
                    break;
                default:
                    SimpleLogger.LogInformationWithCallStack(e.Data, methodNamesFromStack);
                    break;
            }
        }
    }

    private static Process CreateProcess(string executableFileName, string arguments, IFolder workingFolder) {
        return new() {
            StartInfo = {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = executableFileName,
                Arguments = arguments,
                WorkingDirectory = workingFolder.FullName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
    }
}