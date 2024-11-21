using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class VariablesPage : Page
{
    public VariablesViewModel ViewModel
    {
        get;
    }

    public VariablesPage()
    {
        ViewModel = App.GetService<VariablesViewModel>();
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

    private void VariablesListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not Variable variable)
        {
            return;
        }

        ViewModel.NavigateToVariableDetails(variable, false);
    }

    private async void MenuFlyoutItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem item)
        {
            return;
        }

        if (item.DataContext is not Variable variable)
        {
            return;
        }

        if (item.Text == "Edit")
        {
            ViewModel.NavigateToVariableDetails(variable, true);
        }
        else if (item.Text == "Delete")
        {
            var dialog = DeleteConfirmationDialog();

            dialog.PrimaryButtonClick += async (s, e) =>
            {
                await ViewModel.DeleteVariableAsync(variable);
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

    private void NewVariableButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OnNewVariableClicked();
    }
}
