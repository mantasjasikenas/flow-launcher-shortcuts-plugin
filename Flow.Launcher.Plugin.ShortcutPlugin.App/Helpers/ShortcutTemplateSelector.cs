using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
public class ShortcutTemplateSelector : DataTemplateSelector
{
    public DataTemplate FileShortcutTemplate
    {
        get; set;
    }
    public DataTemplate DirectoryShortcutTemplate
    {
        get; set;
    }

    public DataTemplate UrlShortcutTemplate
    {
        get; set;
    }

    public DataTemplate GroupShortcutTemplate
    {
        get; set;
    }

    public DataTemplate ShellShortcutTemplate
    {
        get; set;
    }

    public DataTemplate DefaultTemplate
    {
        get; set;
    }

    protected override DataTemplate SelectTemplateCore(object item) =>
        SelectTemplateCore(item, null);

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        var shortcut = item as ObservableShortcut;

        return shortcut switch
        {
            ObservableFileShortcut => FileShortcutTemplate,
            ObservableDirectoryShortcut => DirectoryShortcutTemplate,
            ObservableUrlShortcut => UrlShortcutTemplate,
            ObservableShellShortcut => ShellShortcutTemplate,
            ObservableGroupShortcut => GroupShortcutTemplate,
            _ => DefaultTemplate
        };
    }
}

