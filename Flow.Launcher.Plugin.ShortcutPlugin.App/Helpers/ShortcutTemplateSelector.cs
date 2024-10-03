using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
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

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var shortcut = item as Shortcut;

        return shortcut switch
        {
            FileShortcut => FileShortcutTemplate,
            DirectoryShortcut => DirectoryShortcutTemplate,
            UrlShortcut => UrlShortcutTemplate,
            ShellShortcut => ShellShortcutTemplate,
            GroupShortcut => GroupShortcutTemplate,
            _ => base.SelectTemplate(item)
        };
    }
}

