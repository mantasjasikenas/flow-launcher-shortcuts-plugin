using Flow.Launcher.Plugin.ShortcutPlugin.App.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class VariableDetailsPage : Page
{
    public VariableDetailsViewModel ViewModel
    {
        get;
        set;
    }

    public VariableDetailsPage()
    {
        ViewModel = App.GetService<VariableDetailsViewModel>();
        InitializeComponent();
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.IsEditMode = true;
    }

    private void ShowErrorButton_Click(object sender, RoutedEventArgs e)
    {
        var errors = ViewModel.Variable.Errors;

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
}
