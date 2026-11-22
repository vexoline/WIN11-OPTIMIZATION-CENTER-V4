# Windows 11 USB and Network Adapter Optimization

PowerShell and WinUI 3 tools for configuring USB power management and supported network adapter settings on Windows.

## WinUI 3 application

The `Win11OptimizationCenter` project is a native WinUI 3 desktop application with English and Polish UI, adapter discovery, profile descriptions, operation status, and logs stored under `%LOCALAPPDATA%\Win11OptimizationCenter\Log`. The portable, self-contained x64 ZIP is published on the [latest release](https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4/releases/latest); extract it and run `Win11OptimizationCenter.exe`.

The application requests administrator approval through Windows UAC when applying changes. Review the selected adapter and profile before confirming.

Profiles:

- **Standard** applies supported USB and network power settings and available driver-specific options.
- **Diagnostic** applies the Standard settings and records a detailed operation log.
- **Test** simulates the operation without changing system settings.
- **Custom** limits network changes to the selected adapter; USB power settings are still processed.

Adapter-specific options depend on the hardware and installed driver. The current script includes settings for Intel Wi-Fi 6E AX210 and Realtek Gaming 2.5GbE / RTL8125 devices. The general network adapter power-management option can affect all adapters.

## PowerShell script

Run `WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1` from an elevated PowerShell session. Do not weaken the machine's execution policy; follow your organization's policy if script execution is restricted.

The script can disable USB Selective Suspend and network adapter power management, and apply supported device-specific settings such as power saving, roaming, throughput, EEE, interrupt moderation, buffers, Wake-on-LAN, speed/duplex, and priority options. Driver names and available options vary by machine.

## Source and legacy package

- [`Win11OptimizationCenter`](Win11OptimizationCenter/) - WinUI 3 source project and portable publishing script.
- [`WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1`](WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1) - standalone optimization script.
- [`WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b_WINUI3.rar`](WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b_WINUI3.rar) - legacy GUI archive.
- [`LICENSE`](LICENSE) - project license.

Before applying system changes, save your work and consider creating a restore point. Optimization settings may affect battery life, roaming behavior, latency, or compatibility; results depend on the adapter and driver.
