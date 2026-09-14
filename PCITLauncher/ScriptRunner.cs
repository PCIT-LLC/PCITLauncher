namespace PCITLauncher;

using System.Diagnostics;

public static class ScriptRunner
{
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PCITLauncher", "logs");

    private static string NextLogFile(ScriptEntry script)
    {
        Directory.CreateDirectory(LogDir);
        var name = Path.GetFileNameWithoutExtension(script.Script);
        return Path.Combine(LogDir, $"{name}-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }

    public static void Run(ScriptEntry script)
    {
        var logFile = NextLogFile(script);

        if (script.Elevated)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -Command \"Start-Transcript -Path '{logFile}' -Append; & '{script.FullPath}'; Stop-Transcript\"",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            };
            Process.Start(psi);
        }
        else
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{script.FullPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string stdout = proc!.StandardOutput.ReadToEnd();
            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            File.WriteAllText(logFile,
                $"[{DateTime.Now}] Exit code: {proc.ExitCode}{Environment.NewLine}" +
                $"--- stdout ---{Environment.NewLine}{stdout}{Environment.NewLine}" +
                $"--- stderr ---{Environment.NewLine}{stderr}");
        }
    }
}
