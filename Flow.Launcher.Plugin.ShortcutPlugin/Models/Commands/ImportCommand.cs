using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ImportCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    private readonly IVariablesService _variablesService;

    public ImportCommand(IShortcutsService shortcutsService, IVariablesService variablesService)
    {
        _shortcutsService = shortcutsService;
        _variablesService = variablesService;
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
               .WithHandler((_, _) =>
                   _shortcutsService.ImportShortcuts()
                                    .Concat(_variablesService.ImportVariables())
                                    .ToList())
               .Build();
    }
}