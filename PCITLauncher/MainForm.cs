namespace PCITLauncher;

using System.Diagnostics;
using System.Text.Json;

public class MainForm : Form
{
    private NotifyIcon _tray = null!;
    private ContextMenuStrip _menu = null!;
    private Mutex _mutex = null!;
    private LauncherConfig _config = null!;
    private ScriptConfig _scriptConfig = new();

    public MainForm()
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCITLauncher", "startup.log");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(logPath, $"[{DateTime.Now}] MainForm ctor start{Environment.NewLine}");
        }
        catch { }

        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        Opacity = 0;
        Size = new Size(1, 1);

        _mutex = new Mutex(true, "PCITLauncher-SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            // Log before showing the message box
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PCITLauncher", "startup.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Single-instance check FAILED - already running{Environment.NewLine}");
            }
            catch { }

            MessageBox.Show("PCITLauncher is already running.", "PCITLauncher",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Environment.Exit(0);
            return;
        }

        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCITLauncher", "startup.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] Single-instance check OK{Environment.NewLine}");
        }
        catch { }

        // Load the PCIT icon from the bundled app.ico; fallback to shield if missing
        var iconPath = Path.Combine(AppContext.BaseDirectory, "app.ico");
        var trayIcon = File.Exists(iconPath)
            ? new System.Drawing.Icon(iconPath)
            : SystemIcons.Shield;

        _tray = new NotifyIcon
        {
            Icon = trayIcon,
            Visible = true,
            Text = "PCITLauncher"
        };

        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCITLauncher", "startup.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] NotifyIcon created{Environment.NewLine}");
        }
        catch { }

        LoadOrCreateConfig();
        InitializeScripts();
        BuildMenu();

        Application.ApplicationExit += (_, _) =>
        {
            _tray.Visible = false;
            _tray.Dispose();
            _mutex.ReleaseMutex();
        };
    }

    private void LoadOrCreateConfig()
    {
        _config = ScriptRepoManager.LoadConfig();
        if (_config == null)
        {
            using var dlg = new Form
            {
                Text = "PCITLauncher Setup",
                Size = new Size(500, 220),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Text = "PCITScripts repo URL:", Location = new Point(12, 15), AutoSize = true };
            var txt = new TextBox { Text = "https://github.com/PCIT-LLC/PCITScripts.git", Location = new Point(12, 40), Size = new Size(460, 23) };
            var lblBranch = new Label { Text = "Branch:", Location = new Point(12, 75), AutoSize = true };
            var txtBranch = new TextBox { Text = "main", Location = new Point(12, 100), Size = new Size(100, 23) };
            var chkAuto = new CheckBox { Text = "Auto-update scripts on startup", Location = new Point(12, 130), Checked = true, AutoSize = true };
            var btn = new Button { Text = "Save", Location = new Point(397, 155), DialogResult = DialogResult.OK };

            dlg.Controls.AddRange(new Control[] { lbl, txt, lblBranch, txtBranch, chkAuto, btn });
            dlg.AcceptButton = btn;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _config = new LauncherConfig
                {
                    RepoUrl = txt.Text.Trim(),
                    Branch = txtBranch.Text.Trim(),
                    AutoUpdate = chkAuto.Checked
                };
                ScriptRepoManager.SaveConfig(_config);
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }

    private void InitializeScripts()
    {
        if (!ScriptRepoManager.IsCloned())
        {
            try
            {
                ScriptRepoManager.Clone(_config.RepoUrl, _config.Branch);
            }
            catch (Exception ex)
            {
                _tray.ShowBalloonTip(5000, "Clone failed", ex.Message, ToolTipIcon.Error);
                return;
            }
        }
        else if (_config.AutoUpdate)
        {
            try
            {
                ScriptRepoManager.Pull(_config.Branch);
            }
            catch { /* non-blocking */ }
        }

        LoadScriptConfig();
    }

    private void LoadScriptConfig()
    {
        var path = ScriptRepoManager.ScriptConfigPath;
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _scriptConfig = JsonSerializer.Deserialize<ScriptConfig>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            foreach (var cat in _scriptConfig.Categories)
            {
                foreach (var script in cat.Scripts)
                {
                    script.FullPath = Path.Combine(ScriptRepoManager.ScriptsDir, script.Script);
                }
            }
        }
    }

    private void BuildMenu()
    {
        _menu = new ContextMenuStrip();

        foreach (var cat in _scriptConfig.Categories)
        {
            var catItem = new ToolStripMenuItem(cat.Name);
            foreach (var script in cat.Scripts)
            {
                var scriptItem = new ToolStripMenuItem(script.Label);
                scriptItem.Image = script.Elevated ? SystemIcons.Shield.ToBitmap() : null;
                scriptItem.Click += (_, _) => ScriptRunner.Run(script);
                catItem.DropDownItems.Add(scriptItem);
            }
            _menu.Items.Add(catItem);
        }

        _menu.Items.Add(new ToolStripSeparator());
        var toolsItem = new ToolStripMenuItem("Tools");

        var updateItem = new ToolStripMenuItem("Check for Script Updates");
        updateItem.Click += (_, _) =>
        {
            try
            {
                bool updated = ScriptRepoManager.Pull(_config.Branch);
                if (updated)
                {
                    LoadScriptConfig();
                    BuildMenu();
                    _tray.ContextMenuStrip = _menu;
                    _tray.ShowBalloonTip(2000, "Updated", "Scripts updated successfully.", ToolTipIcon.Info);
                }
                else
                {
                    MessageBox.Show("Already up to date.", "Script Updates",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}", "Script Updates",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        toolsItem.DropDownItems.Add(updateItem);

        var logsItem = new ToolStripMenuItem("Open Logs Folder");
        logsItem.Click += (_, _) =>
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PCITLauncher", "logs");
            Directory.CreateDirectory(logDir);
            Process.Start("explorer.exe", logDir);
        };
        toolsItem.DropDownItems.Add(logsItem);

        var configItem = new ToolStripMenuItem("Settings...");
        configItem.Click += (_, _) =>
        {
            using var dlg = new Form
            {
                Text = "PCITLauncher Settings",
                Size = new Size(500, 220),
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Text = "PCITScripts repo URL:", Location = new Point(12, 15), AutoSize = true };
            var txt = new TextBox { Text = _config.RepoUrl, Location = new Point(12, 40), Size = new Size(460, 23) };
            var lblBranch = new Label { Text = "Branch:", Location = new Point(12, 75), AutoSize = true };
            var txtBranch = new TextBox { Text = _config.Branch, Location = new Point(12, 100), Size = new Size(100, 23) };
            var chkAuto = new CheckBox { Text = "Auto-update scripts on startup", Location = new Point(12, 130), Checked = _config.AutoUpdate, AutoSize = true };
            var btn = new Button { Text = "Save", Location = new Point(397, 155), DialogResult = DialogResult.OK };

            dlg.Controls.AddRange(new Control[] { lbl, txt, lblBranch, txtBranch, chkAuto, btn });
            dlg.AcceptButton = btn;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _config.RepoUrl = txt.Text.Trim();
                _config.Branch = txtBranch.Text.Trim();
                _config.AutoUpdate = chkAuto.Checked;
                ScriptRepoManager.SaveConfig(_config);
            }
        };
        toolsItem.DropDownItems.Add(configItem);

        _menu.Items.Add(toolsItem);
        _menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Application.Exit();
        _menu.Items.Add(exitItem);

        _tray.ContextMenuStrip = _menu;
    }

    protected override void OnShown(EventArgs e)
    {
        Hide();
        base.OnShown(e);
    }
}
