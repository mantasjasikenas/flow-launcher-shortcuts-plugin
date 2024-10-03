using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

using Windows.System;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;


public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBarBackgroundColors(Microsoft.UI.Colors.Transparent);
        App.MainWindow.SetTitleBar(AppTitleBar);

        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();

        SetupNavigationMenu();
    }

    private void SetupNavigationMenu()
    {
        var home = new NavigationViewItem
        {
            Content = "Shell_Main".GetLocalized(),
            Icon = new SymbolIcon(Symbol.Home),
            Tag = typeof(HomePage)
        };
        var shortcuts = new NavigationViewItem
        {
            Content = "Shell_Shortcuts".GetLocalized(),
            Icon = new SymbolIcon(Symbol.AllApps),
            Tag = typeof(ShortcutsPage),
        };

        NavigationHelper.SetNavigateTo(home, typeof(HomeViewModel).FullName);
        NavigationHelper.SetNavigateTo(shortcuts, typeof(ShortcutsViewModel).FullName);

        NavigationViewControl.MenuItems.Add(home);
        NavigationViewControl.MenuItems.Add(shortcuts);
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        TitleBarHelper.UpdateTitleBar(RequestedTheme);

        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        App.AppTitlebar = AppTitleBar as UIElement;
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }

    private void NavigationView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        if (args.DisplayMode == NavigationViewDisplayMode.Compact || args.DisplayMode == NavigationViewDisplayMode.Minimal)
        {
            PaneToggleBtn.Visibility = Visibility.Visible;
            AppTitleBar.Margin = new Thickness(48, 0, 0, 0);
            AppTitleBarText.Margin = new Thickness(12, 0, 0, 0);
        }
        else
        {
            PaneToggleBtn.Visibility = Visibility.Collapsed;
            AppTitleBar.Margin = new Thickness(16, 0, 0, 0);
            AppTitleBarText.Margin = new Thickness(16, 0, 0, 0);
        }
    }

    private void PaneToggleBtn_Click(object sender, RoutedEventArgs e)
    {
        NavigationViewControl.IsPaneOpen = !NavigationViewControl.IsPaneOpen;
    }
}
