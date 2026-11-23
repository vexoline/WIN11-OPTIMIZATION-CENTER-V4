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

    private async void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        var strings = ViewModel.Strings;
        var content = new StackPanel
        {
            Spacing = 12,
            MaxWidth = 520
        };

        var heading = new Grid { ColumnSpacing = 14 };
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        heading.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var icon = new Border
        {
            Width = 48,
            Height = 48,
            CornerRadius = new Microsoft.UI.Xaml.CornerRadius(12),
            Child = new FontIcon
            {
                Glyph = "\uE839",
                FontSize = 26,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        heading.Children.Add(icon);

        var appDetails = new StackPanel { Spacing = 4, VerticalAlignment = VerticalAlignment.Center };
        appDetails.Children.Add(new TextBlock
        {
            Text = strings.AppTitle,
            FontSize = 19,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });
        appDetails.Children.Add(new TextBlock
        {
            Text = strings.AppSubtitle,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap
        });
        Grid.SetColumn(appDetails, 1);
        heading.Children.Add(appDetails);
        content.Children.Add(heading);

        content.Children.Add(new TextBlock
        {
            Text = strings.VersionLabel,
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });
        content.Children.Add(new TextBlock
        {
            Text = $"{strings.AboutAuthorLabel}: Jacek Zaczek",
            FontSize = 15
        });

        var links = new StackPanel { Spacing = 4 };
        links.Children.Add(new HyperlinkButton
        {
            Content = $"{strings.AboutGitHubLabel}: vexoline/WIN11_SET_ADV_DEV_OPT_V4",
            NavigateUri = new Uri("https://github.com/vexoline/WIN11_SET_ADV_DEV_OPT_V4"),
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 15
        });
        links.Children.Add(new HyperlinkButton
        {
            Content = $"{strings.AboutEmailLabel}: j.zaczek@data.pl",
            NavigateUri = new Uri("mailto:j.zaczek@data.pl"),
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 15
        });
        content.Children.Add(links);

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = strings.AboutTitle,
            Content = content,
            CloseButtonText = strings.Close
        };
        await dialog.ShowAsync();
    }

    private void ProfileHelpButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileHelpTip.IsOpen = !ProfileHelpTip.IsOpen;
    }
}
