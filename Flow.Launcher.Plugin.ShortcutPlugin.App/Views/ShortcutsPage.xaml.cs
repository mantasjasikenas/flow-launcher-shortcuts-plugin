using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Microsoft.UI.Xaml;
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
        var shortcutTypes = Enum.GetValues<ShortcutType>();

        foreach (var type in shortcutTypes)
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

        ViewModel.NavigateToShortcutDetails(shortcut, false);

    }

    private void NewShortcutMenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.DataContext is ShortcutType type)
        {
            ViewModel.OnNewShortcutClicked(type);
        }
    }

    private async void MenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item)
        {
            return;
        }

        if (item.DataContext is not Shortcut shortcut)
        {
            return;
        }

        if (item.Text == "Edit")
        {
            ViewModel.NavigateToShortcutDetails(shortcut, true);
        }
        else if (item.Text == "Duplicate")
        {
            await ViewModel.DuplicateShortcutAsync(shortcut);
        }
        else if (item.Text == "Delete")
        {
            var dialog = DeleteConfirmationDialog();

            dialog.PrimaryButtonClick += async (s, e) =>
            {
                await ViewModel.DeleteShortcutAsync(shortcut);
            };

            await dialog.ShowAsync();
        }
    }

    private ContentDialog DeleteConfirmationDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Are you sure?",
            Content = new TextBlock
            {
                Text = "Do you want to delete this shortcut?",
                TextWrapping = TextWrapping.WrapWholeWords
            },
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = XamlRoot
        };

        return dialog;
    }
}
