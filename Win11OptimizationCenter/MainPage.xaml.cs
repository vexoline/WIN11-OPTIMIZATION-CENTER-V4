using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using Win11OptimizationCenter.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Win11OptimizationCenter;

/// <summary>
/// The main content page displayed inside the application window.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public static Visibility BoolToVisibility(bool value) =>
        value ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility InvertBoolToVisibility(bool value) =>
        value ? Visibility.Collapsed : Visibility.Visible;

    public static bool Not(bool value) => !value;

    public MainPage()
    {
        InitializeComponent();
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Page_Loaded;
        UpdateWindowTitle();
        await ViewModel.ScanCommand.ExecuteAsync(null);
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.Strings))
        {
            UpdateWindowTitle();
        }
    }

    private void UpdateWindowTitle()
    {
        if (App.Window is MainWindow mainWindow)
        {
            mainWindow.SetDisplayLanguage(ViewModel.Strings.WindowTitle, ViewModel.Strings.WindowSubtitle);
        }
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e) => App.Window.Close();

    private void ProfileHelpButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileHelpTip.IsOpen = !ProfileHelpTip.IsOpen;
    }
}
