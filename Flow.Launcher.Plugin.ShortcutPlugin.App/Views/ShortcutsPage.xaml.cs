using CommunityToolkit.WinUI.UI.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
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

        SetupNewShortcutFlyoutMenuItems();
    }

    private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            ViewModel.OnFilterChanged(sender.Text);
        }
    }

    private void SetupNewShortcutFlyoutMenuItems()
    {
        var shorcutTypes = Enum.GetValues<ShortcutType>();

        foreach (var type in shorcutTypes)
        {
            var item = new MenuFlyoutItem
            {
                Text = type.ToString(),
                DataContext = type
            };

            item.Click += NewShortcutMenuFlyoutItem_Click;
            NewShortcutFlyout.Items.Add(item);
        }
    }

    private void ShortcutsListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Shortcut shortcut)
        {
            return;
        }

        ViewModel.OnShortcutClicked(shortcut);

    }

    private void NewShortcutMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ShortcutType type)
        {
            ViewModel.OnNewShortcutClicked(type);
        }
    }
}
