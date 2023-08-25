using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class CommandsService : ICommandsService
{
    private readonly ISettingsService _settingsService;
    private readonly IShortcutsService _shortcutsService;
    private readonly IVariablesService _variablesService;
    private readonly ICommandsRepository _commandsRepository;


    public CommandsService(
        IShortcutsService shortcutsService,
        ISettingsService settingsService,
        IVariablesService variablesService,
        ICommandsRepository commandsRepository
    )
    {
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
        _variablesService = variablesService;
        _commandsRepository = commandsRepository;
    }

    public List<Result> ResolveCommand(List<string> arguments, string query)
    {
        return _commandsRepository.ResolveCommand(arguments, query);
    }

    public void ReloadPluginData()
    {
        _settingsService.Reload();
        _shortcutsService.Reload();
        _variablesService.Reload();
    }
}