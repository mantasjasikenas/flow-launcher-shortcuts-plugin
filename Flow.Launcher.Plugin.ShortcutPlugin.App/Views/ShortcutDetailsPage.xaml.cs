using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class ShortcutDetailsPage : Page
{
    public ShortcutDetailsViewModel ViewModel
    {
        get;
        set;
    }

    public ShortcutDetailsPage()
    {
        ViewModel = App.GetService<ShortcutDetailsViewModel>();
        InitializeComponent();
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsEditMode)
        {
            var result = await ShowSaveDialogAsync();

            if (result == ContentDialogResult.Primary)
            {
            }
            else if (result == ContentDialogResult.Secondary)
            {
            }

            ViewModel.IsEditMode = false;
        }
        else
        {
            // Turning on edit mode
            ViewModel.IsEditMode = true;
        }
    }

    private async Task<ContentDialogResult> ShowSaveDialogAsync()
    {
        var saveDialog = new ContentDialog
        {
            Title = "Save Changes",
            Content = "Do you want to save changes before exiting edit mode?",
            PrimaryButtonText = "Save",
            SecondaryButtonText = "Discard",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        return await saveDialog.ShowAsync();
    }

    private ContentDialog CreateAliasDialog()
    {
        var dialog = new ContentDialog
        {
            Title = "Create Alias",
            Content = new TextBox
            {
                PlaceholderText = "Enter alias name",
                AcceptsReturn = false,
                TextWrapping = TextWrapping.NoWrap,
                MaxLength = 100
            },
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot
        };

        return dialog;
    }

    private async void CreateAliasButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsEditMode)
        {
            return;
        }

        var dialog = CreateAliasDialog();

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var aliasName = ((TextBox)dialog.Content).Text;

            ViewModel.AddAlias(aliasName);
        }
    }

    private void DeleteAliasButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.IsEditMode)
        {
            return;
        }

        var button = (Button)sender;
        var aliasName = (string)button.Tag;

        ViewModel.RemoveAlias(aliasName);
    }
}
