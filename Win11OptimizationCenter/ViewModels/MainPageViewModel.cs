using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Win11OptimizationCenter.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial UiStrings Strings { get; set; } = UiStrings.English;

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; } = "English";

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = UiStrings.English.ReadyStatus;

    [ObservableProperty]
    public partial InfoBarSeverity StatusSeverity { get; set; } = InfoBarSeverity.Informational;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial ProfileOption? SelectedProfile { get; set; }

    [ObservableProperty]
    public partial AdapterStatus? SelectedAdapter { get; set; }

    [ObservableProperty]
    public partial string LastOperation { get; set; } = UiStrings.English.ReadyDetail;

    [ObservableProperty]
    public partial string LogText { get; set; } = UiStrings.English.LogPlaceholder;

    [ObservableProperty]
    public partial bool GenerateLog { get; set; } = true;

    public ObservableCollection<AdapterStatus> Adapters { get; } = new();

    public ObservableCollection<ProfileOption> Profiles { get; } = new();

    public int AdapterCount => Adapters.Count;

    public int ConnectedAdapterCount => Adapters.Count(adapter => adapter.IsConnected);

    public bool HasAdapters => Adapters.Count > 0;

    public string SelectedProfileId => SelectedProfile?.Id ?? "Standardowy";

    public string ProfileDescription => SelectedProfileId switch
    {
        "Diagnostyczny (z logowaniem)" => Strings.DiagnosticDescription,
        "Testowy" => Strings.TestDescription,
        "Custom" when SelectedAdapter is null => Strings.CustomRequiresAdapter,
        "Custom" => string.Format(CultureInfo.CurrentCulture, Strings.CustomDescription, SelectedAdapter!.Name),
        _ => Strings.StandardDescription
    };

    public string ScriptPath =>
        Path.Combine(AppContext.BaseDirectory, "Scripts", "WIN11_SET_ADV_DEV_OPT_NET_USB_PWR_V4b.ps1");

    public string LogDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Win11OptimizationCenter", "Log");

    public string LogPath =>
        Path.Combine(LogDirectory, "WIN11_USB_NETWORK_OPTIMIZATION.log");

    public string LiveLogPath =>
        Path.Combine(LogDirectory, "WIN11_USB_NETWORK_OPTIMIZATION.live.log");

    public string LogLocation => LiveLogPath;

    public MainPageViewModel()
    {
        RebuildProfiles("Standardowy");
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        Strings = value == "Polski" ? UiStrings.Polish : UiStrings.English;
        RebuildProfiles(SelectedProfileId);
        RefreshAdapterLanguage();
        OnPropertyChanged(nameof(ProfileDescription));

        if (!IsBusy)
        {
            StatusMessage = Strings.ReadyStatus;
            LastOperation = Strings.ReadyDetail;
        }

        if (LogText == UiStrings.English.LogPlaceholder || LogText == UiStrings.Polish.LogPlaceholder)
        {
            LogText = Strings.LogPlaceholder;
        }
    }

    partial void OnSelectedProfileChanged(ProfileOption? value)
    {
        OnPropertyChanged(nameof(SelectedProfileId));
        OnPropertyChanged(nameof(ProfileDescription));
    }

    partial void OnSelectedAdapterChanged(AdapterStatus? value)
    {
        OnPropertyChanged(nameof(ProfileDescription));
        if (value is not null && SelectedProfileId != "Custom")
        {
            SelectedProfile = Profiles.FirstOrDefault(profile => profile.Id == "Custom");
        }
    }

    private void RebuildProfiles(string selectedId)
    {
        Profiles.Clear();
        Profiles.Add(new ProfileOption("Standardowy", Strings.StandardProfile));
        Profiles.Add(new ProfileOption("Diagnostyczny (z logowaniem)", Strings.DiagnosticProfile));
        Profiles.Add(new ProfileOption("Testowy", Strings.TestProfile));
        Profiles.Add(new ProfileOption("Custom", Strings.CustomProfile));
        SelectedProfile = Profiles.FirstOrDefault(profile => profile.Id == selectedId) ?? Profiles[0];
    }

    private void RefreshAdapterLanguage()
    {
        var selectedName = SelectedAdapter?.Name;
        for (var index = 0; index < Adapters.Count; index++)
        {
            var adapter = Adapters[index];
            Adapters[index] = adapter with
            {
                Status = adapter.IsConnected ? Strings.Connected : Strings.Disconnected
            };
        }

        if (selectedName is not null)
        {
            SelectedAdapter = Adapters.FirstOrDefault(adapter => adapter.Name == selectedName);
        }
    }

    [RelayCommand]
    private async Task ScanAsync()
    {
        IsBusy = true;
        SelectedAdapter = null;
        Adapters.Clear();
        OnPropertyChanged(nameof(AdapterCount));
        OnPropertyChanged(nameof(ConnectedAdapterCount));
        OnPropertyChanged(nameof(HasAdapters));
        StatusSeverity = InfoBarSeverity.Informational;
        StatusMessage = Strings.ScanningStatus;
        LastOperation = Strings.ScanningDetail;

        try
        {
            const string command =
                "$ErrorActionPreference='Stop'; ConvertTo-Json -InputObject @(Get-NetAdapter -ErrorAction Stop | Select-Object Name, InterfaceDescription, Status, LinkSpeed) -Compress";
            var json = await RunPowerShellAsync(command, TimeSpan.FromSeconds(20));
            var discovered = ParseAdapters(json, Strings);

            Adapters.Clear();
            foreach (var adapter in discovered)
            {
                Adapters.Add(adapter);
            }
            RefreshAdapterLanguage();

            OnPropertyChanged(nameof(AdapterCount));
            OnPropertyChanged(nameof(ConnectedAdapterCount));
            OnPropertyChanged(nameof(HasAdapters));
            StatusSeverity = InfoBarSeverity.Success;
            StatusMessage = Strings.ScanCompletedStatus;
            LastOperation = string.Format(
                CultureInfo.CurrentCulture,
                Strings.ScanSummary,
                AdapterCount,
                ConnectedAdapterCount);
        }
        catch (Exception ex)
        {
            StatusSeverity = InfoBarSeverity.Error;
            StatusMessage = Strings.ScanFailedStatus;
            LastOperation = ex.Message;
            LogText = $"{Strings.ScanError}{Environment.NewLine}{ex}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (!File.Exists(ScriptPath))
        {
            StatusSeverity = InfoBarSeverity.Error;
            StatusMessage = Strings.ScriptMissingStatus;
            LastOperation = ScriptPath;
            return;
        }

        if (SelectedProfileId == "Custom" && SelectedAdapter is null)
        {
            StatusSeverity = InfoBarSeverity.Warning;
            StatusMessage = Strings.SelectAdapterStatus;
            LastOperation = Strings.CustomRequiresAdapter;
            return;
        }

        IsBusy = true;
        StatusSeverity = InfoBarSeverity.Informational;
        StatusMessage = Strings.ApplyingStatus;
        LastOperation = Strings.ConfirmUacDetail;

        try
        {
            Directory.CreateDirectory(LogDirectory);
            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }
            if (File.Exists(LiveLogPath))
            {
                File.Delete(LiveLogPath);
            }

            var shouldGenerateLog = GenerateLog || SelectedProfileId == "Diagnostyczny (z logowaniem)";
            LogText = Strings.LogStarting;

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(ScriptPath)!
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-WindowStyle");
            startInfo.ArgumentList.Add("Hidden");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(ScriptPath);
            startInfo.ArgumentList.Add("-Profile");
            startInfo.ArgumentList.Add(SelectedProfileId);

            if (SelectedProfileId == "Testowy")
            {
                startInfo.ArgumentList.Add("-TestMode");
            }

            if (SelectedProfileId == "Custom")
            {
                startInfo.ArgumentList.Add("-AdapterName");
                startInfo.ArgumentList.Add(SelectedAdapter!.Name);
            }

            startInfo.ArgumentList.Add("-LiveLogPath");
            startInfo.ArgumentList.Add(LiveLogPath);
            startInfo.ArgumentList.Add("-LogDirectory");
            startInfo.ArgumentList.Add(LogDirectory);

            if (shouldGenerateLog)
            {
                startInfo.ArgumentList.Add("-GenerateLog");
            }

            startInfo.ArgumentList.Add("-NonInteractive");

            using var process = Process.Start(startInfo)
                ?? throw new InvalidOperationException(Strings.ProcessStartError);

            LastOperation = string.Format(
                CultureInfo.CurrentCulture,
                Strings.ProfileStartedDetail,
                SelectedProfile?.DisplayName ?? SelectedProfileId);
            var logMonitorTask = MonitorLogAsync(process);
            await process.WaitForExitAsync();
            await logMonitorTask;
            await RefreshLogAsync();

            if (process.ExitCode == 0)
            {
                StatusSeverity = InfoBarSeverity.Success;
                StatusMessage = Strings.ApplyCompletedStatus;
                LastOperation = string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.ProfileCompletedDetail,
                    SelectedProfile?.DisplayName ?? SelectedProfileId);
            }
            else
            {
                StatusSeverity = InfoBarSeverity.Error;
                StatusMessage = Strings.ScriptFailedStatus;
                LastOperation = string.Format(CultureInfo.CurrentCulture, Strings.ExitCodeDetail, process.ExitCode);
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            StatusSeverity = InfoBarSeverity.Warning;
            StatusMessage = Strings.OperationCancelledStatus;
            LastOperation = Strings.UacCancelledDetail;
        }
        catch (Exception ex)
        {
            StatusSeverity = InfoBarSeverity.Error;
            StatusMessage = Strings.ApplyFailedStatus;
            LastOperation = ex.Message;
            LogText = $"{Strings.LaunchError}{Environment.NewLine}{ex}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenLog()
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var target = File.Exists(LogPath) ? LogPath :
                File.Exists(LiveLogPath) ? LiveLogPath : LogDirectory;
            Process.Start(new ProcessStartInfo
            {
                FileName = target,
                UseShellExecute = true
            });
            StatusSeverity = InfoBarSeverity.Informational;
            StatusMessage = target == LogDirectory ? Strings.LogFolderOpenedStatus : Strings.LogOpenedStatus;
            LastOperation = target;
        }
        catch (Exception ex)
        {
            StatusSeverity = InfoBarSeverity.Error;
            StatusMessage = Strings.LogOpenFailedStatus;
            LastOperation = ex.Message;
        }
    }

    private async Task MonitorLogAsync(Process process)
    {
        while (!process.HasExited)
        {
            await RefreshLogAsync();
            await Task.Delay(250);
        }
    }

    private async Task RefreshLogAsync()
    {
        if (!File.Exists(LiveLogPath))
        {
            return;
        }

        await using var stream = new FileStream(
            LiveLogPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 4096,
            useAsync: true);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var content = await reader.ReadToEndAsync();
        if (!string.IsNullOrEmpty(content))
        {
            LogText = content;
        }
    }

    private static async Task<string> RunPowerShellAsync(string command, TimeSpan timeout)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-Command");
        startInfo.ArgumentList.Add(command);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start PowerShell for adapter discovery.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                }
            }

            await process.WaitForExitAsync();
            throw new TimeoutException($"Adapter discovery exceeded {timeout.TotalSeconds:0} seconds.");
        }

        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(error) ? $"PowerShell exited with code {process.ExitCode}." : error.Trim());
        }

        return output;
    }

    private static List<AdapterStatus> ParseAdapters(string json, UiStrings strings)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json);
        var root = document.RootElement;
        var entries = root.ValueKind switch
        {
            JsonValueKind.Array => root.EnumerateArray().ToArray(),
            JsonValueKind.Object => new[] { root },
            _ => Array.Empty<JsonElement>()
        };
        var adapters = new List<AdapterStatus>(entries.Length);

        foreach (var entry in entries)
        {
            var name = ReadJsonString(entry, "Name");
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var description = ReadJsonString(entry, "InterfaceDescription");
            if (string.IsNullOrWhiteSpace(description))
            {
                description = name;
            }

            var connected = ReadJsonString(entry, "Status").Equals("Up", StringComparison.OrdinalIgnoreCase);
            var category = description.Contains("Intel", StringComparison.OrdinalIgnoreCase) ? "Wi-Fi" :
                description.Contains("Realtek", StringComparison.OrdinalIgnoreCase) ? "Ethernet" :
                description.Contains("Cloudflare", StringComparison.OrdinalIgnoreCase) ? "Cloudflare" : "Network";
            var speed = ReadJsonString(entry, "LinkSpeed");

            adapters.Add(new AdapterStatus(
                category,
                name,
                description,
                connected ? strings.Connected : strings.Disconnected,
                string.IsNullOrWhiteSpace(speed) ? strings.NoData : speed,
                connected));
        }

        return adapters;
    }

    private static string ReadJsonString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ToString()
            : string.Empty;
}

public sealed record AdapterStatus(string Category, string Name, string Description, string Status, string Speed, bool IsConnected);

public sealed record ProfileOption(string Id, string DisplayName);

public sealed record UiStrings
{
    public string WindowTitle { get; init; } = string.Empty;
    public string WindowSubtitle { get; init; } = string.Empty;
    public string AppTitle { get; init; } = string.Empty;
    public string AppSubtitle { get; init; } = string.Empty;
    public string AdapterColumn { get; init; } = string.Empty;
    public string StatusColumn { get; init; } = string.Empty;
    public string SpeedColumn { get; init; } = string.Empty;
    public string LanguageLabel { get; init; } = string.Empty;
    public string ScanAgain { get; init; } = string.Empty;
    public string ScanTooltip { get; init; } = string.Empty;
    public string ReadyStatus { get; init; } = string.Empty;
    public string ReadyDetail { get; init; } = string.Empty;
    public string ScanningStatus { get; init; } = string.Empty;
    public string ScanningDetail { get; init; } = string.Empty;
    public string ScanCompletedStatus { get; init; } = string.Empty;
    public string ScanSummary { get; init; } = string.Empty;
    public string ScanFailedStatus { get; init; } = string.Empty;
    public string ScanError { get; init; } = string.Empty;
    public string DetectedAdapters { get; init; } = string.Empty;
    public string ActiveConnections { get; init; } = string.Empty;
    public string PowerScope { get; init; } = string.Empty;
    public string AdaptersTitle { get; init; } = string.Empty;
    public string NoAdapters { get; init; } = string.Empty;
    public string AdapterSelectionTooltip { get; init; } = string.Empty;
    public string ProfileTitle { get; init; } = string.Empty;
    public string ProfilePickerLabel { get; init; } = string.Empty;
    public string StandardProfile { get; init; } = string.Empty;
    public string DiagnosticProfile { get; init; } = string.Empty;
    public string TestProfile { get; init; } = string.Empty;
    public string CustomProfile { get; init; } = string.Empty;
    public string ProfileDescriptionTitle { get; init; } = string.Empty;
    public string StandardDescription { get; init; } = string.Empty;
    public string DiagnosticDescription { get; init; } = string.Empty;
    public string TestDescription { get; init; } = string.Empty;
    public string CustomRequiresAdapter { get; init; } = string.Empty;
    public string CustomDescription { get; init; } = string.Empty;
    public string GenerateLog { get; init; } = string.Empty;
    public string GenerateLogTooltip { get; init; } = string.Empty;
    public string ApplySettings { get; init; } = string.Empty;
    public string ApplyTooltip { get; init; } = string.Empty;
    public string OpenLog { get; init; } = string.Empty;
    public string SecurityTitle { get; init; } = string.Empty;
    public string SecurityDescription { get; init; } = string.Empty;
    public string LogTitle { get; init; } = string.Empty;
    public string AboutTitle { get; init; } = string.Empty;
    public string AboutDescription { get; init; } = string.Empty;
    public string AboutAuthorLabel { get; init; } = string.Empty;
    public string AboutEmailLabel { get; init; } = string.Empty;
    public string AboutGitHubLabel { get; init; } = string.Empty;
    public string VersionLabel { get; init; } = string.Empty;
    public string Close { get; init; } = string.Empty;
    public string CloseTooltip { get; init; } = string.Empty;
    public string ProfileHelpTitle { get; init; } = string.Empty;
    public string ProfileHelpText { get; init; } = string.Empty;
    public string Connected { get; init; } = string.Empty;
    public string Disconnected { get; init; } = string.Empty;
    public string NoData { get; init; } = string.Empty;
    public string LogPlaceholder { get; init; } = string.Empty;
    public string ScriptMissingStatus { get; init; } = string.Empty;
    public string SelectAdapterStatus { get; init; } = string.Empty;
    public string ApplyingStatus { get; init; } = string.Empty;
    public string ConfirmUacDetail { get; init; } = string.Empty;
    public string LogStarting { get; init; } = string.Empty;
    public string LogDisabled { get; init; } = string.Empty;
    public string ProcessStartError { get; init; } = string.Empty;
    public string ProfileStartedDetail { get; init; } = string.Empty;
    public string ApplyCompletedStatus { get; init; } = string.Empty;
    public string ProfileCompletedDetail { get; init; } = string.Empty;
    public string ScriptFailedStatus { get; init; } = string.Empty;
    public string ExitCodeDetail { get; init; } = string.Empty;
    public string OperationCancelledStatus { get; init; } = string.Empty;
    public string UacCancelledDetail { get; init; } = string.Empty;
    public string ApplyFailedStatus { get; init; } = string.Empty;
    public string LaunchError { get; init; } = string.Empty;
    public string LogOpenedStatus { get; init; } = string.Empty;
    public string LogFolderOpenedStatus { get; init; } = string.Empty;
    public string LogOpenFailedStatus { get; init; } = string.Empty;

    public static UiStrings English { get; } = new()
    {
        WindowTitle = "Windows 11 Network Interface & USB Power Management Optimizer",
        WindowSubtitle = "Intel Wi-Fi 6E AX210 · Realtek Gaming 2.5GbE / RTL8125 · USB Selective Suspend",
        AppTitle = "Windows 11 Network Interface & USB Power Management Optimizer",
        AppSubtitle = "Intel Wi-Fi 6E AX210 · Realtek Gaming 2.5GbE / RTL8125 · USB Selective Suspend",
        AdapterColumn = "Adapter",
        StatusColumn = "Status",
        SpeedColumn = "Link speed",
        LanguageLabel = "Language",
        ScanAgain = "Scan again",
        ScanTooltip = "Refresh the detected network adapters.",
        ReadyStatus = "Ready",
        ReadyDetail = "Select a profile, review its description and apply when ready.",
        ScanningStatus = "Scanning network adapters...",
        ScanningDetail = "Reading the current device state.",
        ScanCompletedStatus = "Scan completed",
        ScanSummary = "Found {0} adapters; active connections: {1}.",
        ScanFailedStatus = "Adapter scan failed",
        ScanError = "Network adapter scan error:",
        DetectedAdapters = "Detected adapters",
        ActiveConnections = "Active connections",
        PowerScope = "Power-management scope",
        AdaptersTitle = "Network adapters",
        NoAdapters = "No adapters found. Select Scan again to retry.",
        AdapterSelectionTooltip = "Select an adapter to target it with the Custom profile.",
        ProfileTitle = "Optimization profile",
        ProfilePickerLabel = "Choose a profile",
        StandardProfile = "Standard",
        DiagnosticProfile = "Diagnostic (with logging)",
        TestProfile = "Test",
        CustomProfile = "Custom",
        ProfileDescriptionTitle = "What this profile does",
        StandardDescription = "Applies supported USB selective-suspend and network-adapter power settings. Intel and Realtek options are applied only when exposed by the installed driver. Administrator approval is required.",
        DiagnosticDescription = "Applies the Standard settings and records a detailed operation log in your user profile. Administrator approval is required.",
        TestDescription = "Simulates the operation and reports planned actions without changing system settings.",
        CustomRequiresAdapter = "Select a network adapter first. Custom limits network changes to that adapter; USB power settings are still processed.",
        CustomDescription = "Limits network settings to “{0}”. USB power settings are still processed. Driver-specific options are applied only when available.",
        GenerateLog = "Save detailed log",
        GenerateLogTooltip = "Save the operation log in your local user profile.",
        ApplySettings = "Apply settings",
        ApplyTooltip = "Starts the script after you approve the Windows UAC prompt.",
        OpenLog = "Open log",
        SecurityTitle = "Security",
        SecurityDescription = "Scanning only reads device information. Changes are made only after you select Apply settings and approve the Windows UAC prompt.",
        LogTitle = "Operation details",
        AboutTitle = "About",
        AboutDescription = "Windows 11 network-adapter and USB power-management utility.",
        AboutAuthorLabel = "Author",
        AboutEmailLabel = "Email",
        AboutGitHubLabel = "GitHub repository",
        VersionLabel = "Version 1.1.0",
        Close = "Close",
        CloseTooltip = "Close the application.",
        ProfileHelpTitle = "Optimization profiles",
        ProfileHelpText = "Standard — applies supported USB and network-adapter settings.\nDiagnostic — applies Standard settings and saves a detailed log.\nTest — reports planned actions without changing settings.\nCustom — limits network changes to the selected adapter; USB settings are still processed.",
        Connected = "Connected",
        Disconnected = "Disconnected",
        NoData = "No data",
        LogPlaceholder = "Optimization details will appear here after you start a scan or apply a profile.",
        ScriptMissingStatus = "Optimization script not found",
        SelectAdapterStatus = "Select a network adapter",
        ApplyingStatus = "Applying optimization...",
        ConfirmUacDetail = "Approve the Windows UAC prompt to continue. PowerShell runs in the background; progress appears here.",
        LogStarting = "Starting the script. Waiting for the first log entry...",
        LogDisabled = "Starting the script. Detailed logging is disabled.",
        ProcessStartError = "Could not start the PowerShell process.",
        ProfileStartedDetail = "Started the “{0}” profile.",
        ApplyCompletedStatus = "Optimization completed",
        ProfileCompletedDetail = "The “{0}” profile finished.",
        ScriptFailedStatus = "The script reported an error",
        ExitCodeDetail = "Exit code: {0}. Review the operation log for details.",
        OperationCancelledStatus = "Operation cancelled",
        UacCancelledDetail = "The UAC prompt was not approved; settings were not changed.",
        ApplyFailedStatus = "Could not run the optimization",
        LaunchError = "Script launch error:",
        LogOpenedStatus = "Log opened",
        LogFolderOpenedStatus = "Log folder opened",
        LogOpenFailedStatus = "Could not open the log"
    };

    public static UiStrings Polish { get; } = new()
    {
        WindowTitle = "Optymalizator kart sieciowych i zasilania USB — Windows 11",
        WindowSubtitle = "Intel Wi-Fi 6E AX210 · Realtek Gaming 2.5GbE / RTL8125 · oszczędzanie energii USB",
        AppTitle = "Optymalizator kart sieciowych i zasilania USB — Windows 11",
        AppSubtitle = "Intel Wi-Fi 6E AX210 · Realtek Gaming 2.5GbE / RTL8125 · oszczędzanie energii USB",
        AdapterColumn = "Karta sieciowa",
        StatusColumn = "Stan",
        SpeedColumn = "Szybkość łącza",
        LanguageLabel = "Język",
        ScanAgain = "Skanuj ponownie",
        ScanTooltip = "Ponownie wykrywa karty sieciowe.",
        ReadyStatus = "Gotowe",
        ReadyDetail = "Wybierz profil, przeczytaj jego opis i uruchom, gdy będziesz gotowy.",
        ScanningStatus = "Skanowanie kart sieciowych...",
        ScanningDetail = "Odczyt bieżącego stanu urządzeń.",
        ScanCompletedStatus = "Skanowanie zakończone",
        ScanSummary = "Wykryto kart: {0}; aktywne połączenia: {1}.",
        ScanFailedStatus = "Skanowanie kart nie powiodło się",
        ScanError = "Błąd skanowania kart sieciowych:",
        DetectedAdapters = "Wykryte karty",
        ActiveConnections = "Aktywne połączenia",
        PowerScope = "Zakres zarządzania energią",
        AdaptersTitle = "Karty sieciowe",
        NoAdapters = "Nie wykryto kart. Wybierz „Skanuj ponownie”, aby spróbować jeszcze raz.",
        AdapterSelectionTooltip = "Wybierz kartę, aby ograniczyć do niej działanie profilu Custom.",
        ProfileTitle = "Profil optymalizacji",
        ProfilePickerLabel = "Wybierz profil",
        StandardProfile = "Standardowy",
        DiagnosticProfile = "Diagnostyczny (z logowaniem)",
        TestProfile = "Testowy",
        CustomProfile = "Custom",
        ProfileDescriptionTitle = "Działanie wybranego profilu",
        StandardDescription = "Stosuje obsługiwane ustawienia zasilania USB i kart sieciowych. Opcje Intel i Realtek są zmieniane tylko wtedy, gdy udostępnia je zainstalowany sterownik. Wymaga zgody administratora.",
        DiagnosticDescription = "Stosuje ustawienia profilu Standardowego i zapisuje szczegółowy log w profilu użytkownika. Wymaga zgody administratora.",
        TestDescription = "Symuluje operację i przedstawia planowane czynności bez zmieniania ustawień systemu.",
        CustomRequiresAdapter = "Najpierw wybierz kartę sieciową. Profil Custom ogranicza zmiany sieciowe do tej karty; ustawienia zasilania USB nadal są przetwarzane.",
        CustomDescription = "Ogranicza ustawienia sieciowe do karty „{0}”. Ustawienia zasilania USB nadal są przetwarzane. Opcje sterownika są stosowane tylko wtedy, gdy są dostępne.",
        GenerateLog = "Zapisuj szczegółowy log",
        GenerateLogTooltip = "Zapisuje log operacji w profilu lokalnym użytkownika.",
        ApplySettings = "Zastosuj ustawienia",
        ApplyTooltip = "Uruchamia skrypt po zaakceptowaniu monitu UAC systemu Windows.",
        OpenLog = "Otwórz log",
        SecurityTitle = "Bezpieczeństwo",
        SecurityDescription = "Skanowanie tylko odczytuje informacje o urządzeniach. Zmiany zostaną wykonane dopiero po wybraniu „Zastosuj ustawienia” i zaakceptowaniu monitu UAC.",
        LogTitle = "Szczegóły wykonania",
        AboutTitle = "Informacje",
        AboutDescription = "Narzędzie Windows 11 do zarządzania energią kart sieciowych i USB.",
        AboutAuthorLabel = "Autor",
        AboutEmailLabel = "E-mail",
        AboutGitHubLabel = "Repozytorium GitHub",
        VersionLabel = "Wersja 1.1.0",
        Close = "Zamknij",
        CloseTooltip = "Zamyka aplikację.",
        ProfileHelpTitle = "Profile optymalizacji",
        ProfileHelpText = "Standardowy — stosuje obsługiwane ustawienia USB i kart sieciowych.\nDiagnostyczny — stosuje ustawienia standardowe i zapisuje szczegółowy log.\nTestowy — przedstawia planowane czynności bez zmieniania ustawień.\nCustom — ogranicza zmiany sieciowe do wybranej karty; ustawienia USB nadal są przetwarzane.",
        Connected = "Połączono",
        Disconnected = "Rozłączono",
        NoData = "Brak danych",
        LogPlaceholder = "Szczegóły optymalizacji pojawią się tutaj po skanowaniu lub zastosowaniu profilu.",
        ScriptMissingStatus = "Nie znaleziono skryptu optymalizacji",
        SelectAdapterStatus = "Wybierz kartę sieciową",
        ApplyingStatus = "Trwa optymalizacja...",
        ConfirmUacDetail = "Zaakceptuj monit UAC systemu Windows. PowerShell działa w tle, a postęp jest widoczny tutaj.",
        LogStarting = "Uruchamianie skryptu. Oczekiwanie na pierwszy wpis logu...",
        LogDisabled = "Uruchamianie skryptu. Szczegółowy log jest wyłączony.",
        ProcessStartError = "Nie udało się uruchomić procesu PowerShell.",
        ProfileStartedDetail = "Uruchomiono profil „{0}”.",
        ApplyCompletedStatus = "Optymalizacja zakończona",
        ProfileCompletedDetail = "Profil „{0}” zakończył działanie.",
        ScriptFailedStatus = "Skrypt zgłosił błąd",
        ExitCodeDetail = "Kod zakończenia: {0}. Szczegóły znajdziesz w logu operacji.",
        OperationCancelledStatus = "Operacja anulowana",
        UacCancelledDetail = "Monit UAC nie został zaakceptowany; ustawienia nie zostały zmienione.",
        ApplyFailedStatus = "Nie udało się uruchomić optymalizacji",
        LaunchError = "Błąd uruchomienia skryptu:",
        LogOpenedStatus = "Otwarto log",
        LogFolderOpenedStatus = "Otwarto folder logów",
        LogOpenFailedStatus = "Nie udało się otworzyć logu"
    };
}
