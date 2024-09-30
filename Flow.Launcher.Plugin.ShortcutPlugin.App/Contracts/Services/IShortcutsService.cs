using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;

public interface IShortcutsService
{
    Task<IEnumerable<Shortcut>> GetShortcutsAsync();
}
