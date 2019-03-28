using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Extensions;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class SimpleLogFlusher : ISimpleLogFlusher {
        private static readonly object LockObject = new object();
        public HashSet<string> FileNames { get; } = new HashSet<string>();

        public void Flush(ISimpleLogger logger) {
            lock (LockObject) {
                var logEntries = logger.FindLogEntries(e => !e.Flushed);
                var ids = logEntries.Select(e => e.Stack[0]).Distinct().ToList();
                var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubLogs");
                folder.CreateIfNecessary();
                foreach (var id in ids) {
                    var fileName = folder.FullName + '\\' + id + ".log";
                    var entries = logEntries.Where(e => !e.Flushed && e.Stack[0] == id).ToList();
                    File.AppendAllLines(fileName, entries.Select(Format));
                    logger.OnEntriesFlushed(entries);
                    FileNames.Add(fileName);
                }
            }
        }

        private static string Format(ISimpleLogEntry entry) {
            return entry.LogTime.ToString("yyyy-MM-dd") + '\t'
                                                        + entry.LogTime.ToString("HH:mm:ss.ffff") + '\t'
                                                        + string.Join("-", entry.Stack) + '\t'
                                                        + Enum.GetName(typeof(LogLevel), entry.LogLevel) + '\t'
                                                        + entry.Message;
        }
    }
}
