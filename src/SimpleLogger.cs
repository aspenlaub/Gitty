using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Entities;
using Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty {
    public class SimpleLogger : ISimpleLogger {
        public IList<ISimpleLogEntry> LogEntries { get; }
        private readonly IList<string> vStack;
        private readonly ISimpleLogFlusher vSimpleLogFlusher;

        public SimpleLogger(ISimpleLogFlusher simpleLogFlusher) {
            LogEntries = new List<ISimpleLogEntry>();
            vStack = new List<string>();
            vSimpleLogFlusher = simpleLogFlusher;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            LogEntries.Add(SimpleLogEntry.Create(logLevel, vStack, formatter(state, exception)));
            vSimpleLogFlusher.Flush(this);
        }

        public bool IsEnabled(LogLevel logLevel) {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) {
            var loggingScope = state as ISimpleLoggingScopeId;
            if (loggingScope == null) {
                return new LoggingScope(() => { });
            }

            var stackEntry = $"{loggingScope.Class}({loggingScope.Id})";
            vStack.Add(stackEntry);
            return new LoggingScope(() => { vStack.Remove(stackEntry); });
        }
    }
}
