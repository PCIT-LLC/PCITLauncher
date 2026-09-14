using PCITLauncher;

[STAThread]
static void Main()
{
    try
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCITLauncher", "startup.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.AppendAllText(logPath, $"[{DateTime.Now}] Process started{Environment.NewLine}");

        ApplicationConfiguration.Initialize();
        File.AppendAllText(logPath, $"[{DateTime.Now}] ApplicationConfiguration.Initialize() OK{Environment.NewLine}");

        Application.Run(new MainForm());
        File.AppendAllText(logPath, $"[{DateTime.Now}] Application.Run() returned{Environment.NewLine}");
    }
    catch (Exception ex)
    {
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCITLauncher", "startup.log");
        Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
        File.AppendAllText(logPath, $"[{DateTime.Now}] FATAL: {ex}{Environment.NewLine}");
    }
}
