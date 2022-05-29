using System.Collections.Generic;
using System.Linq;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Extensions;

public static class ErrorsAndInfosExtension {
    public static string ErrorsPlusRelevantInfos(this IErrorsAndInfos errorsAndInfos) {
        if (!errorsAndInfos.AnyErrors()) { return ""; }

        var components = new List<string>();
        components.AddRange(errorsAndInfos.Errors);
        components.AddRange(errorsAndInfos.Infos.Where(i => (i.StartsWith("Failed") || i.Contains(".trx")) && !components.Contains(i)));
        return string.Join("\r\n", components);
    }
}