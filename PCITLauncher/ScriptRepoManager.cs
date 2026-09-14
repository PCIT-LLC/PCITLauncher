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

        // Fetch all refs from origin
        var remote = repo.Network.Fetch("origin", new string[] { $"+refs/heads/{branch}:refs/remotes/origin/{branch}" });

        // Get local and remote branch tips
        var localBranch = repo.Branches[branch];
        var remoteBranch = repo.Branches[$"origin/{branch}"];

        if (localBranch == null || remoteBranch == null)
            return false;

        // If local is behind remote, update
        if (localBranch.Tip.Id != remoteBranch.Tip.Id)
        {
            // Move the local branch to the remote tip
            repo.Reset(ResetMode.Hard, remoteBranch.Tip);
            return true;
        }

        return false; // Already up to date
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
