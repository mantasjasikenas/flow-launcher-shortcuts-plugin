using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;

public interface IShortcutsService
{
    Task<IEnumerable<Shortcut>> GetShortcutsAsync();
    Task SaveShortcutAsync(Shortcut shortcut);
    Task DeleteShortcutAsync(Shortcut shortcut);
    Task UpdateShortcutAsync(Shortcut oldShortcut, Shortcut updatedShortcut);
    Task RefreshShortcutsAsync();
}
