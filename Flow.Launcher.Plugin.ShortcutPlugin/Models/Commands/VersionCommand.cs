﻿using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class VersionCommand : ICommand
{
    private readonly PluginInitContext _context;

    public VersionCommand(PluginInitContext context)
    {
        _context = context;
    }

    public Command Create()
    {
        return CreateVersionCommand();
    }

    private Command CreateVersionCommand()
    {
        return new CommandBuilder()
               .WithKey("version")
               .WithResponseInfo(("version", "Show plugin version"))
               .WithResponseFailure(("Failed to show plugin version", "Something went wrong"))
               .WithResponseSuccess(("Version", "Show plugin version"))
               .WithHandler(VersionCommandHandler)
               .Build();
    }

    private List<Result> VersionCommandHandler(ActionContext context, List<string> arguments)
    {
        var version = _context.CurrentPluginMetadata.Version;

        return ResultExtensions.SingleResult("Plugin version", version);
    }
}