using System;
using System.IO;

try
{
    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    var logPath = Path.Combine(desktop, "pcitest.log");
    File.WriteAllText(logPath, $"[{DateTime.Now}] .NET TEST OK{Environment.NewLine}CLR: {Environment.Version}{Environment.NewLine}OS: {Environment.OSVersion}{Environment.NewLine}Machine: {Environment.MachineName}{Environment.NewLine}");
    Console.WriteLine("SUCCESS - check Desktop\\pcitest.log");
}
catch (Exception ex)
{
    Console.WriteLine($"FAILED: {ex.Message}");
}
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
