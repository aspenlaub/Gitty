using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ISimpleLogger : ILogger {
        IList<ISimpleLogEntry> FindLogEntries(Func<ISimpleLogEntry, bool> condition);
        void OnEntriesFlushed(IList<ISimpleLogEntry> entries);
    }
}
