using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ExportCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;

    private readonly IVariablesService _variablesService;

    public ExportCommand(IShortcutsService shortcutsService, IVariablesService variablesService)
    {
        _shortcutsService = shortcutsService;
        _variablesService = variablesService;
    }

    public Command Create()
    {
        return CreateExportCommand();
    }

    private Command CreateExportCommand()
    {
        return new CommandBuilder()
               .WithKey("export")
               .WithResponseInfo(("export", "Export shortcuts"))
               .WithResponseFailure(("Failed to export shortcuts", "Something went wrong"))
               .WithHandler((_, _) => _shortcutsService.ExportShortcuts()
                                                       .Concat(_variablesService.ExportVariables())
                                                       .ToList()
               )
               .Build();
    }
}