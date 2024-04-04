using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ExportCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;


    public ExportCommand(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
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
            .WithHandler((_, _) => _shortcutsService.ExportShortcuts())
            .Build();
    }
}