using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
