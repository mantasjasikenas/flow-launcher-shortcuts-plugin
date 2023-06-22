using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Handlers;

public interface IPathHandler
{
    ShortcutType ResolveShortcutType(string rawPath);
    void OpenShortcut(Shortcut shortcut);
}