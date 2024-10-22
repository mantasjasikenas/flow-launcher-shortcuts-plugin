using Flow.Launcher.Plugin.ShortcutPlugin.App.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void OpenColorsSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var uri = new Uri("ms-settings:colors");

            _ = Windows.System.Launcher.LaunchUriAsync(uri);
        }
        catch (Exception)
        {
        }
    }

    private async void PickShortcutPathButton_Click(object sender, RoutedEventArgs e)
    {
        var filePicker = Pickers.CreateFilePicker([".json"]);

        var file = await filePicker.PickSingleFileAsync();

        if (file == null)
        {
            return;
        }

        await ViewModel.SetShortcutsPath(file.Path);
    }

    private async void PickVariablePathButton_Click(object sender, RoutedEventArgs e)
    {
        var filePicker = Pickers.CreateFilePicker([".json"]);

        var file = await filePicker.PickSingleFileAsync();

        if (file == null)
        {
            return;
        }

        await ViewModel.SetVariablesPath(file.Path);
    }
}