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
        private static DateTime vCleanupTime = DateTime.Now;
        public HashSet<string> FileNames { get; } = new HashSet<string>();

        public void Flush(ISimpleLogger logger) {
            var folder = new Folder(Path.GetTempPath()).SubFolder("AspenlaubLogs");
            folder.CreateIfNecessary();

            lock (LockObject) {
                var logEntries = logger.FindLogEntries(e => !e.Flushed);
                var ids = logEntries.Select(e => e.Stack[0]).Distinct().ToList();
                foreach (var id in ids) {
                    var fileName = folder.FullName + '\\' + id + ".log";
                    var entries = logEntries.Where(e => !e.Flushed && e.Stack[0] == id).ToList();
                    File.AppendAllLines(fileName, entries.Select(Format));
                    logger.OnEntriesFlushed(entries);
                    FileNames.Add(fileName);
                }
            }

            if (DateTime.Now < vCleanupTime) { return; }

            var minWriteTime = DateTime.Now.AddDays(-1);
            var files = Directory.GetFiles(folder.FullName, "*.log", SearchOption.TopDirectoryOnly).Where(f => File.GetLastWriteTime(f) < minWriteTime).ToList();
            foreach (var file in files) {
                File.Delete(file);
            }

            vCleanupTime = DateTime.Now.AddHours(2);
        }

        private static string Format(ISimpleLogEntry entry) {
            return entry.LogTime.ToString("yyyy-MM-dd") + '\t'
                                                        + entry.LogTime.ToString("HH:mm:ss.ffff") + '\t'
                                                        + string.Join("-", entry.Stack) + '\t'
                                                        + Enum.GetName(typeof(LogLevel), entry.LogLevel) + '\t'
                                                        + entry.Message;
        }

        internal void ResetCleanupTime() {
            vCleanupTime = DateTime.Now;
        }
    }
}
