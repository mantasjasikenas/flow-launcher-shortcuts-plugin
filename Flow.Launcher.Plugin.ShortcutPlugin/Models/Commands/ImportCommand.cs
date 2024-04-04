using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ImportCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    public ImportCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
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