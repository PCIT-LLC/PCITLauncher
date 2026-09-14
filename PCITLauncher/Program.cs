using PCITLauncher;

[STAThread]
static void Main()
{
    ApplicationConfiguration.Initialize();
    Application.Run(new MainForm());
}
