using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;

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

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsEditMode = true;
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

    private void ShowErrorButton_Click(object sender, RoutedEventArgs e)
    {
        var errors = ViewModel.Shortcut.Errors;

        if (string.IsNullOrEmpty(errors))
        {
            return;
        }

        var flyout = new Flyout
        {
            Content = new TextBlock
            {
                Text = errors,
                TextWrapping = TextWrapping.WrapWholeWords
            }
        };

        flyout.ShowAt((Button)sender);
    }

    private async void PickIconButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = CreateFilePicker(Constants.IconsExtensions);

        var file = await picker.PickSingleFileAsync();

        if (file == null)
        {
            return;
        }

        var iconPath = file.Path;

        ViewModel.Shortcut.Icon = iconPath;
    }

    private async void PickDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = CreateFolderPicker();

        var directoru = await picker.PickSingleFolderAsync();

        if (directoru == null)
        {
            return;
        }

        if (ViewModel.Shortcut is ObservableDirectoryShortcut shortcut)
        {
            shortcut.Path = directoru.Path;
        }
    }

    private async void PickFileButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = CreateFilePicker();

        var file = await picker.PickSingleFileAsync();

        if (file == null)
        {
            return;
        }

        if (ViewModel.Shortcut is ObservableFileShortcut shortcut)
        {
            shortcut.Path = file.Path;
        }
    }

    private FileOpenPicker CreateFilePicker(List<string>? fileTypeFilter = null)
    {
        var filePicker = new FileOpenPicker();

        var window = App.MainWindow;

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

        filePicker.ViewMode = PickerViewMode.Thumbnail;

        if (fileTypeFilter is null)
        {
            filePicker.FileTypeFilter.Add("*");
        }
        else
        {
            foreach (var filter in fileTypeFilter)
            {
                filePicker.FileTypeFilter.Add(filter);
            }
        }

        return filePicker;
    }

    private FolderPicker CreateFolderPicker()
    {
        var folderPicker = new FolderPicker();

        var window = App.MainWindow;

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        folderPicker.ViewMode = PickerViewMode.Thumbnail;

        return folderPicker;
    }
}
