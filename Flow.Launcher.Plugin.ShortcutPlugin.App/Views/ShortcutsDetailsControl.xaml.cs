using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Views;

public sealed partial class ShortcutsDetailsView : UserControl
{
    public Shortcut? ShortcutsDetailsMenuItem
    {
        get => GetValue(ShortcutsDetailsMenuItemProperty) as Shortcut;
        set => SetValue(ShortcutsDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ShortcutsDetailsMenuItemProperty = DependencyProperty.Register("ShortcutsDetailsMenuItem", typeof(Shortcut), typeof(ShortcutsDetailsView), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ShortcutsDetailsView()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShortcutsDetailsView control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
