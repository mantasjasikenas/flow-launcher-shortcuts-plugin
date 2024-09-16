using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(DirectoryShortcut), nameof(ShortcutType.Directory))]
[JsonDerivedType(typeof(UrlShortcut), nameof(ShortcutType.Url))]
[JsonDerivedType(typeof(FileShortcut), nameof(ShortcutType.File))]
[JsonDerivedType(typeof(ShellShortcut), nameof(ShortcutType.Shell))]
[JsonDerivedType(typeof(GroupShortcut), nameof(ShortcutType.Group))]
public abstract class Shortcut : ICloneable
{
    public string Key { get; set; }

    public List<string> Alias { get; set; }

    public string Description { get; set; }

    public string Icon { get; set; }

    public string GetDerivedType()
    {
        return this switch
        {
            FileShortcut => "File",
            DirectoryShortcut => "Directory",
            UrlShortcut => "Url",
            GroupShortcut => "Group",
            ShellShortcut => "Shell",
            _ => "Unspecified shortcut type"
        };
    }

    public string GetTitle()
    {
        var title = $"{Key}{GetAlias()}";

        return string.IsNullOrWhiteSpace(title) ? GetDerivedType() : title;
    }

    public string GetSubTitle()
    {
        return ToString();
    }

    private string GetAlias()
    {
        // Alternative symbols: ⨯ ⇒ ⪢ ⌗
        return Alias is {Count: > 0} ? $" ⌗ {string.Join(" ⌗ ", Alias)}" : string.Empty;
    }


    public abstract object Clone();
}