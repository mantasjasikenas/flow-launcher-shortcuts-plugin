using System;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(PluginShortcut), nameof(ShortcutType.Plugin))]
[JsonDerivedType(typeof(ProgramShortcut), nameof(ShortcutType.Program))]
[JsonDerivedType(typeof(UrlShortcut), nameof(ShortcutType.Url))]
[JsonDerivedType(typeof(DirectoryShortcut), nameof(ShortcutType.Directory))]
[JsonDerivedType(typeof(FileShortcut), nameof(ShortcutType.File))]
[JsonDerivedType(typeof(GroupShortcut), nameof(ShortcutType.Group))]
[JsonDerivedType(typeof(ShellShortcut), nameof(ShortcutType.Shell))]
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
            ProgramShortcut => "Program",
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
            ProgramShortcut => Icons.Terminal,
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
        ProgramShortcut => ShortcutType.Program,
        ShellShortcut => ShortcutType.Shell,
        PluginShortcut => ShortcutType.Plugin,
        GroupShortcut => ShortcutType.Group,
        _ => ShortcutType.Unspecified
    };


    public abstract object Clone();
}