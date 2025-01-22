using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

public interface IIconProvider : IAsyncInitializable
{
    string GetIcon(Shortcut shortcut);
    void Reload();
}