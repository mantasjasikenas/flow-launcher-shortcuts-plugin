namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public record Variable
{
    public string Name { get; init; }

    public string Value { get; init; }
}