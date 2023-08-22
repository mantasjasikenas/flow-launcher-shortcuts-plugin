using System;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(PluginShortcut), nameof(ShortcutType.Plugin))]
[JsonDerivedType(typeof(ProgramShortcut), nameof(ShortcutType.Program))]
[JsonDerivedType(typeof(UrlShortcut), nameof(ShortcutType.Url))]
[JsonDerivedType(typeof(DirectoryShortcut), nameof(ShortcutType.Directory))]
[JsonDerivedType(typeof(FileShortcut), nameof(ShortcutType.File))]
[JsonDerivedType(typeof(GroupShortcut), nameof(ShortcutType.Group))]
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
            _ => "Unspecified"
        };
    }

    public ShortcutType GetShortcutType() => this switch
    {
        FileShortcut => ShortcutType.File,
        DirectoryShortcut => ShortcutType.Directory,
        UrlShortcut => ShortcutType.Url,
        ProgramShortcut => ShortcutType.Program,
        PluginShortcut => ShortcutType.Plugin,
        GroupShortcut => ShortcutType.Group,
        _ => ShortcutType.Unspecified
    };


    public abstract object Clone();
}