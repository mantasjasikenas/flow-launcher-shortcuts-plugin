using System;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ImportCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;
    private readonly ISettingsService _settingsService;

    public ImportCommand(IShortcutsService shortcutsService, ISettingsService settingsService)
    {
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
    }

    public Command Create()
    {
        return CreateImportCommand();
    }

    private Command CreateImportCommand()
    {
        return new CommandBuilder()
            .WithKey("import")
            .WithResponseInfo(("import", "Import shortcuts"))
            .WithResponseFailure(("Failed to import shortcuts", "Something went wrong"))
            .WithHandler((_, _) => _shortcutsService.ImportShortcuts())
            .Build();
    }
}