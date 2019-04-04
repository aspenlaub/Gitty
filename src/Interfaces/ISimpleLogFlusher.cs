using System.Collections.Generic;
// ReSharper disable UnusedMember.Global

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Interfaces {
    public interface ISimpleLogFlusher {
        HashSet<string> FileNames { get; }

        void Flush(ISimpleLogger logger);
    }
}
