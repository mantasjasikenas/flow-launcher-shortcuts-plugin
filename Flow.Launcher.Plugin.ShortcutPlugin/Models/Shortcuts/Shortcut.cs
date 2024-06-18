using System;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(DirectoryShortcut), nameof(ShortcutType.Directory))]
[JsonDerivedType(typeof(UrlShortcut), nameof(ShortcutType.Url))]
[JsonDerivedType(typeof(FileShortcut), nameof(ShortcutType.File))]
[JsonDerivedType(typeof(ShellShortcut), nameof(ShortcutType.Shell))]
[JsonDerivedType(typeof(GroupShortcut), nameof(ShortcutType.Group))]
[JsonDerivedType(typeof(PluginShortcut), nameof(ShortcutType.Plugin))]
public abstract class Shortcut : ICloneable
{
    public string Key { get; set; }

    public string GetDerivedType()
    {
        return this switch
        {
            FileShortcut => "File",
            DirectoryShortcut => "Directory",
            UrlShortcut => "Url",
            PluginShortcut => "Plugin",
            GroupShortcut => "Group",
            ShellShortcut => "Shell",
            _ => "Unspecified shortcut type"
        };
    }

    public string GetIcon()
    {
        return this switch
        {
            FileShortcut => Icons.File,
            DirectoryShortcut => Icons.Folder,
            UrlShortcut => Icons.Link,
            ShellShortcut => Icons.Terminal,
            PluginShortcut => Icons.Logo,
            GroupShortcut => Icons.TabGroup,
            _ => Icons.Logo
        };
    }

    public ShortcutType GetShortcutType() => this switch
    {
        FileShortcut => ShortcutType.File,
        DirectoryShortcut => ShortcutType.Directory,
        UrlShortcut => ShortcutType.Url,
        ShellShortcut => ShortcutType.Shell,
        PluginShortcut => ShortcutType.Plugin,
        GroupShortcut => ShortcutType.Group,
        _ => ShortcutType.Unspecified
    };


    public abstract object Clone();
}