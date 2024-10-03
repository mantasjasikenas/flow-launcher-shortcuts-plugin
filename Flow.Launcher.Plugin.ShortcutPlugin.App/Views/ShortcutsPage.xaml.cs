using CommunityToolkit.WinUI.UI.Controls;

using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class ShortcutsPage : Page
{
    public ShortcutsViewModel ViewModel
    {
        get;
    }

    public ShortcutsPage()
    {
        ViewModel = App.GetService<ShortcutsViewModel>();
        InitializeComponent();
        ViewModel.AutoSuggestBox = SearchBox;
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.OnFilterChanged(sender.Text);
        }
    }
}
