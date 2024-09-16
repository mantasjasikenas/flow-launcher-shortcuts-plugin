using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class HelpCommand : ICommand
{
    private readonly PluginInitContext _context;

    public HelpCommand(PluginInitContext context)
    {
        _context = context;
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

    private List<Result> HelpCommandHandler(ActionContext context, List<string> arguments)
    {
        var readmeResult = ResultExtensions.Result(
            "Open the plugin's documentation",
            Constants.ReadmeUrl,
            () => { ShortcutUtilities.OpenUrl(Constants.ReadmeUrl); },
            iconPath: Icons.Info,
            autoCompleteText: Constants.ReadmeUrl
        );

        var reportIssueResult = ResultExtensions.Result(
            "Report an issue on GitHub",
            Constants.GithubIssues,
            () => { ShortcutUtilities.OpenUrl(Constants.GithubIssues); },
            iconPath: Icons.Github,
            autoCompleteText: Constants.GithubIssues
        );

        var discordResult = ResultExtensions.Result(
            "Contact the developer on Discord",
            "Username: " + Constants.DiscordUsername,
            () => { _context.API.CopyToClipboard(Constants.DiscordUsername); },
            iconPath: Icons.Discord,
            autoCompleteText: Constants.DiscordUsername
        );

        return new List<Result> {readmeResult, reportIssueResult, discordResult};
    }
}