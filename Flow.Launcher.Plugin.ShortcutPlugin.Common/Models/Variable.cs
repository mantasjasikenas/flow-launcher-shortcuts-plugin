namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

public record Variable
{
    public string Name { get; init; }

    public string Value { get; init; }
}