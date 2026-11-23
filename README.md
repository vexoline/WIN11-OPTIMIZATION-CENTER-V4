<div align="center">

# Windows 11 Network & USB Optimizer

### A bilingual WinUI 3 control center and PowerShell toolkit for USB power and supported network adapters.

[![Windows 11](https://img.shields.io/badge/Windows-11-0078D4?logo=windows&logoColor=white)](#requirements)
[![WinUI 3](https://img.shields.io/badge/GUI-WinUI%203-6B4FBB)](#winui-3-app)
[![Languages](https://img.shields.io/badge/Languages-English%20%2F%20Polish-107C10)](#features)
[![Architecture](https://img.shields.io/badge/Architecture-x64-6A5ACD)](#requirements)
[![License](https://img.shields.io/badge/License-GPL--3.0-2EA44F)](LICENSE)

<p>
  <a href="https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases/latest">
    <img alt="Download the WinUI 3 app" src="https://img.shields.io/badge/Download-WinUI%203%20x64-107C10?style=for-the-badge&logo=windows&logoColor=white">
  </a>
  <a href="https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases">
    <img alt="View releases" src="https://img.shields.io/badge/View-Release%20Notes-0078D4?style=for-the-badge&logo=github&logoColor=white">
  </a>
  <a href="WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1">
    <img alt="Open the PowerShell script" src="https://img.shields.io/badge/Open-PowerShell%20Script-5391FE?style=for-the-badge&logo=powershell&logoColor=white">
  </a>
</p>

**Discover adapters. Choose a profile. Follow live progress while it runs.**

</div>

---

## ✨ At a glance

| ⚡ WinUI 3 app | 🛠️ PowerShell | 🌐 English + Polski |
|---|---|---|
| Native Windows interface with adapter discovery, profiles, live status, and logs. | Direct access to the underlying USB and network configuration script. | The app starts in English; switch to Polish from the language selector. |

The portable x64 app is published as a **self-contained ZIP**. Extract it and launch `Win11OptimizationCenter.exe`; no separate .NET installation is required.

## 🚀 WinUI 3 app

The dashboard helps you inspect physical network adapters and choose how to apply the supported settings. It includes:

- a selectable adapter list with detected and connected adapter counts;
- an English/Polish language selector and profile descriptions that update with your selection;
- an About dialog with app version, author, GitHub, and email links;
- operation status and a scrolling live-output panel;
- a hidden PowerShell window; progress is streamed to the app while the UAC approval prompt remains visible;
- light, dark, and high-contrast theme support;
- logs saved to `%LOCALAPPDATA%\Win11OptimizationCenter\Log`.

### Choose a profile

| Profile | What it does |
|---|---|
| **Standard** | Applies supported USB and network power settings, plus available driver-specific options. |
| **Diagnostic** | Applies the Standard settings and records a detailed operation log. |
| **Test** | Simulates the operation without changing system settings. |
| **Custom** | Limits network changes to the selected adapter. USB power settings are still processed. |

### Download and run

1. Select **Download WinUI 3 x64** above or open [the latest release](https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases/latest).
2. Extract the ZIP and start `Win11OptimizationCenter.exe`.
3. Select an adapter and profile, then review the profile description.
4. Choose **Apply** and approve the Windows UAC prompt only if you intend to make the changes.

## ⚙️ Supported settings

The script can disable USB Selective Suspend and Windows network-adapter power management, and configure supported options for selected drivers.

| Scope / adapter | Examples of settings |
|---|---|
| **USB** | USB Selective Suspend. |
| **Network adapters** | Windows device power management. |
| **Intel Wi-Fi 6E AX210** | Device power management, MIMO power save, transmit power, roaming aggressiveness, throughput booster, and U-APSD. |
| **Realtek Gaming 2.5GbE / RTL8125** | EEE / Green Ethernet, interrupt moderation, power saving, receive/transmit buffers, Wake-on-LAN, speed and duplex, and priority/VLAN options. |

> **Hardware matters:** driver names and available options vary. The general network power-management setting can affect all adapters; device-specific settings depend on the installed adapter and driver.

## 🧰 PowerShell quick start

Run the script from an elevated PowerShell session:

```powershell
.\WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1
```

The script does not change your execution policy. If script execution is restricted by your organization, follow its policy instead of weakening system-wide protections.

## 🧑‍💻 Build from source

The WinUI 3 project uses .NET 10, Windows App SDK, and CommunityToolkit.Mvvm. From the repository root, run:

```powershell
winapp run .\Win11OptimizationCenter\Win11OptimizationCenter.csproj --arch x64 --debug-output
```

To create a portable, self-contained ZIP:

```powershell
.\Win11OptimizationCenter\Publish-Portable.ps1 -CreateZip -SelfContained
```

The published self-contained build includes its runtime. The smaller non-self-contained package requires the .NET 10 Desktop Runtime and Windows App SDK runtime.

## 🛡️ Safety and compatibility

- Save your work before applying system-wide settings.
- Consider creating a Windows restore point first.
- Changes may affect battery life, roaming, latency, or device compatibility; improved performance is not guaranteed.
- Review the selected adapter and profile, and read the output after applying changes.
- Driver-specific options are not available on every device.

## 📦 Repository contents

| Item | Description |
|---|---|
| [`Win11OptimizationCenter/`](Win11OptimizationCenter/) | WinUI 3 application source and portable publishing script. |
| [`WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1`](WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1) | Standalone PowerShell optimization script. |
| [`WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b_WINUI3.rar`](WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b_WINUI3.rar) | Legacy GUI archive. |
| [Releases](https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases) | Portable x64 application and release notes. |
| [`LICENSE`](LICENSE) | GNU General Public License v3.0. |

## 📋 Requirements

- Windows 11, 64-bit (x64), for the published GUI.
- Administrator approval through UAC to apply system-level settings.
- .NET 10 SDK and the WinApp CLI to build and run the source project.

---

<div align="center">

### ⚡ Tune thoughtfully. Keep control of every change.

**Windows 11 · WinUI 3 · PowerShell · English / Polski**

[![Get the app](https://img.shields.io/badge/Get%20the%20App-Latest%20Release-107C10?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases/latest)

</div>
