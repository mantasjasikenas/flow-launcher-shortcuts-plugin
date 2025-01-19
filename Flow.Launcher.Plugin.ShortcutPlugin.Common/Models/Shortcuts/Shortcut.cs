using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(DirectoryShortcut), nameof(ShortcutType.Directory))]
[JsonDerivedType(typeof(UrlShortcut), nameof(ShortcutType.Url))]
[JsonDerivedType(typeof(FileShortcut), nameof(ShortcutType.File))]
[JsonDerivedType(typeof(ShellShortcut), nameof(ShortcutType.Shell))]
[JsonDerivedType(typeof(GroupShortcut), nameof(ShortcutType.Group))]
[JsonDerivedType(typeof(SnippetShortcut), nameof(ShortcutType.Snippet))]
public abstract class Shortcut : ICloneable
{
    public string Key
    {
        get;
        set;
    }

    public List<string> Alias
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public string Icon
    {
        get;
        set;
    }


    public abstract object Clone();

    public string GetDerivedType()
    {
        return this switch
        {
            FileShortcut => "File",
            DirectoryShortcut => "Directory",
            UrlShortcut => "Url",
            GroupShortcut => "Group",
            ShellShortcut => "Shell",
            SnippetShortcut => "Snippet",
            _ => "Unspecified shortcut type"
        };
    }

    public ShortcutType GetShortcutType() => this switch
    {
        FileShortcut => ShortcutType.File,
        DirectoryShortcut => ShortcutType.Directory,
        UrlShortcut => ShortcutType.Url,
        GroupShortcut => ShortcutType.Group,
        ShellShortcut => ShortcutType.Shell,
        SnippetShortcut => ShortcutType.Snippet,
        _ => throw new ArgumentOutOfRangeException(nameof(ShortcutType), "Shortcut type not found")
    };

    public string GetTitle()
    {
        var title = $"{Key}{GetAlias()}";

        return string.IsNullOrWhiteSpace(title) ? GetDerivedType() : title;
    }

    public virtual string GetSubTitle() => ToString();

    private string GetAlias()
    {
        // Alternative symbols: ⨯ ⇒ ⪢ ⌗
        return Alias is {Count: > 0} ? $" ⌗ {string.Join(" ⌗ ", Alias)}" : string.Empty;
    }
}