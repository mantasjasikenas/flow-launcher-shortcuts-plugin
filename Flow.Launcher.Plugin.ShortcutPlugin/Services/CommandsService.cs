using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
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

    public List<Result> ResolveCommand(List<string> arguments, Query query)
    {
        var results = _commandsRepository.ResolveCommand(arguments, query);

        //TODO: Move this to different place?
        results.ForEach(result =>
        {
            if (string.IsNullOrEmpty(result.AutoCompleteText))
            {
                result.AutoCompleteText = $"{query.ActionKeyword} {result.Title}";
            }
            else if (result.ContextData is Shortcut)
            {
                result.AutoCompleteText = result.SubTitle;
            }
            else
            {
                result.AutoCompleteText = $"{query.ActionKeyword} {result.AutoCompleteText}";
            }
        });

        return results;
    }

    public void ReloadPluginData()
    {
        _settingsService.Reload();
        _shortcutsService.Reload();
        _variablesService.Reload();
    }
}