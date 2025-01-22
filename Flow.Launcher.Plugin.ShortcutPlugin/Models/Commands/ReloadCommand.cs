using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ReloadCommand : ICommand
{
    private readonly IPluginManager _pluginManager;

    public ReloadCommand(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public Command Create()
    {
        return CreateReloadCommand();
    }

    private Command CreateReloadCommand()
    {
        return new CommandBuilder()
               .WithKey("reload")
               .WithResponseInfo(("reload", "Reload plugin data"))
               .WithResponseSuccess(("Reload", "Reload plugin data"))
               .WithResponseFailure(("Failed to reload", "Something went wrong"))
               .WithHandler(ReloadCommandHandler)
               .Build();
    }

    private List<Result> ReloadCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        return ResultExtensions.SingleResult("Reload plugin data", "This action will reload all plugin data",
            asyncAction: async () =>
            {
                await _pluginManager.ReloadDataAsync();
            });
    }
}