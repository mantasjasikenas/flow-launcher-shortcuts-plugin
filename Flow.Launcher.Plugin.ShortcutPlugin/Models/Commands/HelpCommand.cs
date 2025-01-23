using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class HelpCommand : ICommand
{
    private readonly IPluginManager _pluginManager;

    public HelpCommand(IPluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public Command Create()
    {
        return CreateHelpCommand();
    }

    private Command CreateHelpCommand()
    {
        return new CommandBuilder()
               .WithKey("help")
               .WithResponseInfo(("help", "Show help"))
               .WithResponseFailure(("Failed to show help", "Something went wrong"))
               .WithHandler(HelpCommandHandler)
               .Build();
    }

    private List<Result> HelpCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var readmeResult = ResultExtensions.Result(
            "Open the plugin's documentation",
            Constants.ReadmeUrl,
            () => { Helper.ShortcutUtilities.OpenUrl(Constants.ReadmeUrl); },
            iconPath: Icons.Info,
            autoCompleteText: Constants.ReadmeUrl
        );

        var reportIssueResult = ResultExtensions.Result(
            "Report an issue on GitHub",
            Constants.GithubIssues,
            () => { Helper.ShortcutUtilities.OpenUrl(Constants.GithubIssues); },
            iconPath: Icons.Github,
            autoCompleteText: Constants.GithubIssues
        );

        var discordResult = ResultExtensions.Result(
            "Contact the developer on Discord",
            "Username: " + Constants.DiscordUsername,
            () => { _pluginManager.API.CopyToClipboard(Constants.DiscordUsername); },
            iconPath: Icons.Discord,
            autoCompleteText: Constants.DiscordUsername
        );

        return [readmeResult, reportIssueResult, discordResult];
    }
}