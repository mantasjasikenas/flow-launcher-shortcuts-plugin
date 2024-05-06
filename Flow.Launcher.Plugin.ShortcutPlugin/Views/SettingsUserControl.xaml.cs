using Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Views;

public partial class SettingsUserControl
{
    public SettingsUserControl(SettingsViewModel viewModel)
    {
        DataContext = viewModel;

        InitializeComponent();
    }
}