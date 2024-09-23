using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

public interface IIconProvider
{
    string GetIcon(Shortcut shortcut);
    void Reload();
}