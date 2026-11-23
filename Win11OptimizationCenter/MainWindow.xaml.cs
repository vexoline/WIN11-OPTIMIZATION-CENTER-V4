using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.ViewManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Win11OptimizationCenter;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(nint hWnd);

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        ApplyTitleBarContrast();
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
        var workArea = displayArea.WorkArea;
        var scale = GetDpiForWindow(hwnd) / 96.0;
        if (scale <= 0)
        {
            scale = 1;
        }

        var width = Math.Min((int)Math.Round(1240 * scale), (int)(workArea.Width * 0.92));
        var height = Math.Min((int)Math.Round(900 * scale), (int)(workArea.Height * 0.9));
        AppWindow.Resize(new SizeInt32(width, height));
        AppWindow.Move(new PointInt32(
            workArea.X + (workArea.Width - width) / 2,
            workArea.Y + (workArea.Height - height) / 2));

        // Navigate the root frame to the main page on startup.
        RootFrame.Navigate(typeof(MainPage));
    }

    public void SetDisplayLanguage(string title, string subtitle)
    {
        Title = title;
        AppTitleBar.Title = title;
        AppTitleBar.Subtitle = subtitle;
    }

    private void ApplyTitleBarContrast()
    {
        var accent = new UISettings().GetColorValue(UIColorType.Accent);
        var luminance = (0.2126 * accent.R + 0.7152 * accent.G + 0.0722 * accent.B) / 255.0;
        var foreground = luminance > 0.58
            ? Color.FromArgb(255, 10, 20, 32)
            : Color.FromArgb(255, 255, 255, 255);
        var inactiveForeground = luminance > 0.58
            ? Color.FromArgb(210, 10, 20, 32)
            : Color.FromArgb(210, 255, 255, 255);
        var titleBar = AppWindow.TitleBar;

        titleBar.BackgroundColor = accent;
        titleBar.InactiveBackgroundColor = accent;
        titleBar.ButtonBackgroundColor = accent;
        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(45, foreground.R, foreground.G, foreground.B);
        titleBar.ButtonPressedBackgroundColor = Color.FromArgb(70, foreground.R, foreground.G, foreground.B);
        titleBar.ButtonForegroundColor = foreground;
        titleBar.ButtonHoverForegroundColor = foreground;
        titleBar.ButtonPressedForegroundColor = foreground;
        titleBar.ButtonInactiveForegroundColor = inactiveForeground;
        TitleBarSurface.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(accent);
        AppTitleBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(foreground);
    }

}
