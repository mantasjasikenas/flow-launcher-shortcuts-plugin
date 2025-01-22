namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class SnippetShortcut : Shortcut
{
    public string Value
    {
        get;
        set;
    }

    public override object Clone()
    {
        return new SnippetShortcut
        {
            Value = Value,
            Key = Key,
            Alias = Alias,
            Description = Description,
            Icon = Icon
        };
    }

    public override string GetSubTitle()
    {
        return Value.ReplaceLineEndings(" ↩ ");
    }

    public override string ToString()
    {
        return Value;
    }
}