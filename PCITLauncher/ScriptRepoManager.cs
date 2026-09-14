namespace PCITLauncher;

using System.Diagnostics;
using System.Text.Json;

public static class ScriptRepoManager
{
    private static readonly string AppDataDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PCITLauncher");
    public static readonly string ScriptsDir = Path.Combine(AppDataDir, "scripts");
    private static readonly string GitMarker = Path.Combine(ScriptsDir, ".git");
    private static readonly string ConfigPath = Path.Combine(AppDataDir, "launcher.config.json");

    public static bool IsGitInstalled()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc!.WaitForExit();
            return proc.ExitCode == 0;
        }
        catch { return false; }
    }

    public static bool IsCloned() => Directory.Exists(GitMarker);

    public static void Clone(string repoUrl, string branch)
    {
        Directory.CreateDirectory(AppDataDir);
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"-c core.autocrlf=true clone --branch {branch} --single-branch \"{repoUrl}\" \"{ScriptsDir}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        RunGit(psi, "clone");
    }

    public static bool Pull(string branch)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"-C \"{ScriptsDir}\" pull origin {branch}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        var output = RunGit(psi, "pull");
        return output.Contains("Already up to date") || output.Contains("Fast-forward") || output.Contains("files changed");
    }

    private static string RunGit(ProcessStartInfo psi, string operation)
    {
        using var proc = Process.Start(psi);
        string stdout = proc!.StandardOutput.ReadToEnd();
        string stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0)
            throw new Exception($"git {operation} failed: {stderr}");
        return stdout + stderr;
    }

    public static void SaveConfig(LauncherConfig config)
    {
        Directory.CreateDirectory(AppDataDir);
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }

    public static LauncherConfig? LoadConfig()
    {
        if (!File.Exists(ConfigPath)) return null;
        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<LauncherConfig>(json);
    }

    public static string ScriptConfigPath => Path.Combine(ScriptsDir, "config.json");
}
