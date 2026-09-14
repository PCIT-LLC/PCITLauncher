# PCITLauncher

System tray PowerShell script launcher for Windows. Pulls scripts from a centralized [PCITScripts](https://github.com/PCIT-LLC/PCITScripts) repo and presents them in a nested context menu.

## Quick Start

1. Download `PCITLauncher.exe` from the [Releases](https://github.com/YourOrg/PCITLauncher/releases) page.
2. Copy it to any folder on the target machine (e.g., `C:\Tools\PCITLauncher\`).
3. Run it. On first launch, enter the **PCITScripts repo URL** when prompted.
4. The app minimizes to the system tray. Right-click the shield icon to see your scripts.

## Requirements

- Windows 10 or 11
- [git](https://git-scm.com/downloads) installed and in `PATH`
- PowerShell 5.1+ (built into Windows)

## First-Run Setup

When you launch `PCITLauncher.exe` for the first time, a setup dialog appears:

| Field | Description |
|---|---|
| **PCITScripts repo URL** | HTTPS or SSH URL of your PCITScripts repo |
| **Branch** | Branch to track (default: `main`) |
| **Auto-update on startup** | If checked, pulls latest scripts every time the app launches |

These settings are saved to `%LOCALAPPDATA%\PCITLauncher\launcher.config.json`. Scripts are cloned to `%LOCALAPPDATA%\PCITLauncher\scripts\`.

## Using the Launcher

- **Right-click** the tray icon → nested category menus → click a script to run
- **Shield icon** on a menu item = requires admin (UAC prompt will appear)
- **Tools → Check for Script Updates** → manual `git pull` from the scripts repo
- **Tools → Open Logs Folder** → opens `%LOCALAPPDATA%\PCITLauncher\logs\`
- **Tools → Settings** → change the repo URL

## Logs

Every script execution writes a timestamped log file to `%LOCALAPPDATA%\PCITLauncher\logs\`:

- **Non-elevated scripts:** stdout, stderr, and exit code
- **Elevated scripts:** full PowerShell transcript (all streams captured via `Start-Transcript`)

## Updating

- **Launcher itself:** download the new release .exe and replace.
- **Scripts:** click **Tools → Check for Script Updates** or rely on auto-update at startup.

## Troubleshooting

| Problem | Fix |
|---|---|
| "git is not installed" | Install [git](https://git-scm.com/downloads) and restart the launcher |
| Scripts menu is empty | Run **Tools → Check for Script Updates** to force a clone/pull |
| UAC prompt every time | Expected — elevated scripts always trigger UAC by design |
| Ghost icon after crash | Hover over the tray icon — Windows clears it. If persistent, restart Explorer. |

## Building from Source

```bash
git clone https://github.com/PCIT-LLC/PCITLauncher.git
cd PCITLauncher
dotnet publish PCITLauncher/PCITLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

The compiled .exe is in `dist\PCITLauncher.exe`.

## Releasing

Tag a version and push:

```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions builds a self-contained single-file .exe and attaches it to the release automatically.
