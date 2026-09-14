using System;
using System.IO;
using System.Windows.Forms;

namespace PCITLauncher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PCITLauncher", "startup.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, $"[{DateTime.Now}] Main() started{Environment.NewLine}");

                Application.EnableVisualStyles();
                File.AppendAllText(logPath, $"[{DateTime.Now}] EnableVisualStyles OK{Environment.NewLine}");

                Application.SetCompatibleTextRenderingDefault(false);
                File.AppendAllText(logPath, $"[{DateTime.Now}] SetCompatibleTextRenderingDefault OK{Environment.NewLine}");

                var form = new MainForm();
                File.AppendAllText(logPath, $"[{DateTime.Now}] MainForm created{Environment.NewLine}");

                Application.Run(form);
                File.AppendAllText(logPath, $"[{DateTime.Now}] Run returned{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PCITLauncher", "startup.log");
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                File.AppendAllText(logPath, $"[{DateTime.Now}] FATAL in Main: {ex}{Environment.NewLine}");
            }
        }
    }
}
