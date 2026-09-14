try
{
    var logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PCITLauncher", "test.log");
    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
    File.AppendAllText(logPath, $"[{DateTime.Now}] TEST STARTED{Environment.NewLine}");
    File.AppendAllText(logPath, $"[{DateTime.Now}] CLR: {Environment.Version}{Environment.NewLine}");
    File.AppendAllText(logPath, $"[{DateTime.Now}] OS: {Environment.OSVersion}{Environment.NewLine}");
    Console.WriteLine("Test succeeded — check test.log in LocalAppData\\PCITLauncher");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex}");
}
Console.ReadKey();
