using LibGit2Sharp;

namespace Aspenlaub.Net.GitHub.CSharp.Gitty.Components;

public class MasterMaind {
    public static bool IsMainOrMaster(string branchName) {
        return branchName == "master" || branchName == "main";
    }

    public static Branch RemoteMainOrMasterBranch(BranchCollection branchCollection) {
        return branchCollection["origin/main"] ?? branchCollection["origin/master"];
    }
}