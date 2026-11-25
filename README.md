<div align="center">

# Windows 11 Optimization Center v4

**English:** A Windows 11 WinUI 3 control center and PowerShell toolkit for USB power management and driver-specific tuning of Intel Wi-Fi 6E AX210 and Realtek RTL8125 2.5GbE network adapters.

**Polski:** Aplikacja dla systemu Windows 11 łącząca centrum sterowania WinUI 3 ze skryptem PowerShell do zarządzania zasilaniem USB oraz konfiguracji kart Intel Wi-Fi 6E AX210 i Realtek RTL8125 2.5GbE.

[![Windows 11](https://img.shields.io/badge/Windows-11-0078D4?logo=windows&logoColor=white)](#requirements)
[![WinUI 3](https://img.shields.io/badge/GUI-WinUI%203-6B4FBB)](#winui-3-app)
[![Languages](https://img.shields.io/badge/Languages-English%20%2F%20Polish-107C10)](#features)
[![Architecture](https://img.shields.io/badge/Architecture-x64-6A5ACD)](#requirements)
[![License](https://img.shields.io/badge/License-GPL--3.0-2EA44F)](LICENSE)

<p>
  <a href="https://github.com/vexoline/windows-11-optimization-center/releases/latest">
    <img alt="Download the WinUI 3 app" src="https://img.shields.io/badge/Download-WinUI%203%20x64-107C10?style=for-the-badge&logo=windows&logoColor=white">
  </a>
  <a href="https://github.com/vexoline/windows-11-optimization-center/releases">
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

1. Open [the latest release](https://github.com/vexoline/windows-11-optimization-center/releases/latest) and download `Win11OptimizationCenter-v4-x64.zip`.
2. Extract the complete ZIP to a folder and start `Win11OptimizationCenter.exe` from that folder. Keep the supporting files alongside it.
3. Select an adapter and profile, then review the profile description.
4. Choose **Apply** and approve the Windows UAC prompt only if you intend to make the changes.

## ⚙️ Supported settings

The script disables USB Selective Suspend for both AC and battery power in the active power plan. It also disables the per-device power-management option on detected USB hubs. Windows network-adapter power management is handled separately and can apply to all adapters or, with the Custom profile, only the selected adapter.

| Scope / adapter | Examples of settings |
|---|---|
| **USB** | USB Selective Suspend (AC/DC); per-device power management on detected USB hubs. |
| **Network adapters** | Windows network-adapter power management; applied globally or to the selected adapter. |
| **Intel Wi-Fi 6E AX210 160 MHz** | Device power management disabled; MIMO Power Save = No SMPS; Transmit Power = Highest; Roaming Aggressiveness = Lowest; Throughput Booster = Enabled; U-APSD = Disabled. |
| **Realtek RTL8125 2.5GbE** | EEE / Green Ethernet / Gigabit Lite and Interrupt Moderation = Disabled; Power Saving Mode = Disabled; RX/TX Buffers = 4096; shutdown Wake-on-LAN, magic-packet and pattern-match wake = Disabled; WOL shutdown link speed = Not Speed Down; EEE Max Support Speed and Speed & Duplex = 2.5 Gbps Full Duplex; Priority & VLAN = Priority Enabled. |

Driver-specific properties are changed only when the installed driver exposes a matching option. The script targets the AX210 and RTL8125 variants above; this does not imply support for every Intel Wi-Fi or Realtek Ethernet adapter.

**Polski:** Skrypt wyłącza funkcję USB Selective Suspend dla zasilania sieciowego i bateryjnego w aktywnym planie zasilania, a także zarządzanie energią dla wykrytych koncentratorów USB. Zarządzanie energią kart sieciowych jest osobnym ustawieniem — może obejmować wszystkie karty albo tylko wybraną kartę w profilu Custom. Opcje sterownika dotyczą wyłącznie kart Intel Wi-Fi 6E AX210 160 MHz i Realtek RTL8125 2.5GbE oraz są stosowane tylko wtedy, gdy udostępnia je zainstalowany sterownik.

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

## 📦 Project files and release downloads

| Item | Description |
|---|---|
| [`Win11OptimizationCenter/`](Win11OptimizationCenter/) | WinUI 3 application source, assets, and portable publishing script. |
| [`WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1`](WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1) | Standalone PowerShell optimization script. |
| [`Win11OptimizationCenter-v4-x64.zip`](https://github.com/vexoline/windows-11-optimization-center/releases/download/winui-en-pl-2026-11-23-v4/Win11OptimizationCenter-v4-x64.zip) | Current self-contained x64 release. Extract the full archive and run `Win11OptimizationCenter.exe`; the executable needs its accompanying runtime and support files. |
| [`SHA256SUMS.txt`](https://github.com/vexoline/windows-11-optimization-center/releases/download/winui-en-pl-2026-11-23-v4/SHA256SUMS.txt) | SHA-256 checksum for the release ZIP. |
| [All releases](https://github.com/vexoline/windows-11-optimization-center/releases) | Previous versions and release notes. |
| [`LICENSE`](LICENSE) | GNU General Public License v3.0. |

## 📋 Requirements

- Windows 11, 64-bit (x64), for the published GUI.
- Administrator approval through UAC to apply system-level settings.
- .NET 10 SDK and the WinApp CLI to build and run the source project.

---

<div align="center">

### ⚡ Tune thoughtfully. Keep control of every change.

**Windows 11 · WinUI 3 · PowerShell · English / Polski**

[![Get the app](https://img.shields.io/badge/Get%20the%20App-Latest%20Release-107C10?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/vexoline/windows-11-optimization-center/releases/latest)

</div>
