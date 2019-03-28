using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ISimpleLogger : ILogger {
        IList<ISimpleLogEntry> LogEntries { get; }
    }
}
