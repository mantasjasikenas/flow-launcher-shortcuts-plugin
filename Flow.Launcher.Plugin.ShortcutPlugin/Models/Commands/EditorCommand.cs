using System.Collections.Generic;
using System.IO;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

// ReSharper disable once UnusedType.Global
public class EditorCommand : ICommand
{
    private readonly IPluginManager _pluginManager;

    public EditorCommand(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public Command Create()
    {
        return CreateVersionCommand();
    }

    private Command CreateVersionCommand()
    {
        return new CommandBuilder()
               .WithKey("editor")
               .WithResponseInfo(("editor", "Open the editor"))
               .WithResponseFailure(("Failed to open the editor", "Something went wrong"))
               .WithResponseSuccess(("editor", "Open the editor"))
               .WithHandler(EditorCommandHandler)
               .Build();
    }

    private List<Result> EditorCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var pluginDirectory = _pluginManager.Metadata.PluginDirectory;

        var editorPath = Path.Combine(pluginDirectory, "App/Shortcuts.exe");

        if (!File.Exists(editorPath))
        {
            return ResultExtensions.SingleResult("Editor not found", "The editor app was not found");
        }

        return ResultExtensions.SingleResult("Open the editor", "This will open the editor app", action: () =>
        {
            if (File.Exists(editorPath))
            {
                Cli
                    .Wrap(editorPath)
                    .WithWorkingDirectory(pluginDirectory)
                    .ExecuteAsync();
            }
        });
    }
}