namespace PCITLauncher;

using LibGit2Sharp;

public static class ScriptRepoManager
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PCITLauncher");
    public static readonly string ScriptsDir = Path.Combine(AppDataDir, "scripts");
    private static readonly string GitMarker = Path.Combine(ScriptsDir, ".git");
    private static readonly string ConfigPath = Path.Combine(AppDataDir, "launcher.config.json");

    // --- Repo lifecycle ---

    public static bool IsCloned() => Directory.Exists(GitMarker);

    public static void Clone(string repoUrl, string branch)
    {
        Directory.CreateDirectory(AppDataDir);
        Repository.Clone(repoUrl, ScriptsDir, new CloneOptions
        {
            BranchName = branch,
            Checkout = true
        });
    }

    public static bool Pull(string branch)
    {
        using var repo = new Repository(ScriptsDir);
        
        // Fetch from origin
        var remote = repo.Remotes["origin"];
        var refSpecs = remote.FetchRefSpecs;
        repo.Network.Fetch(remote, refSpecs);

        // Check if there are incoming commits
        var localBranch = repo.Branches[branch];
        var remoteBranch = repo.Branches[$"origin/{branch}"];
        
        if (localBranch == null || remoteBranch == null)
            return false;

        // Count commits between local and remote
        var aheadBehind = repo.ObjectDatabase.CalculateAheadBehind(
            localBranch.Tip.Id, remoteBranch.Tip.Id);

        if (aheadBehind.Behind == 0)
            return false; // Already up to date

        // Fast-forward or merge
        var mergeResult = repo.Merge(remoteBranch.Tip, new MergeOptions
        {
            CommitOnSuccess = true,
            FastForwardStrategy = FastForwardStrategy.Default,
            MergeFileFavor = MergeFileFavor.Theirs
        });

        return mergeResult.Status != MergeStatus.Conflicts;
    }

    // --- Config persistence ---

    public static void SaveConfig(LauncherConfig config)
    {
        Directory.CreateDirectory(AppDataDir);
        var json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    public static LauncherConfig? LoadConfig()
    {
        if (!File.Exists(ConfigPath)) return null;
        var json = File.ReadAllText(ConfigPath);
        return System.Text.Json.JsonSerializer.Deserialize<LauncherConfig>(json);
    }

    public static string ScriptConfigPath => Path.Combine(ScriptsDir, "config.json");
}
