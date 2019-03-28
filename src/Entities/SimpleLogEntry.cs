using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Entities {
    public class SimpleLogEntry : ISimpleLogEntry {
        public DateTime LogTime { get; set; }
        public LogLevel LogLevel { get; set; }
        public List<string> Stack { get; set; }
        public string Message { get; set; }
        public bool Flushed { get; set; }

        public static ISimpleLogEntry Create(LogLevel logLevel, IList<string> stack, string message) {
            var entry = new SimpleLogEntry {
                LogTime = DateTime.Now,
                LogLevel = logLevel,
                Stack = new List<string>(),
                Message = message,
                Flushed = false
            };
            entry.Stack.AddRange(stack);
            return entry;
        }
    }
}
