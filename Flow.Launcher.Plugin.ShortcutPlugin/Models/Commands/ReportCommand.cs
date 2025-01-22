using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class ReportCommand : ICommand
{
    public Command Create()
    {
        return CreateReportCommand();
    }

    private Command CreateReportCommand()
    {
        return new CommandBuilder()
               .WithKey("report")
               .WithResponseInfo(("report", "Report an issue"))
               .WithResponseFailure(("Failed to open report", "Something went wrong"))
               .WithHandler(ReportCommandHandler)
               .Build();
    }

    private static List<Result> ReportCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        return ResultExtensions.SingleResult(
            "Report an issue",
            "",
            () => { Helper.ShortcutUtilities.OpenUrl(Constants.GithubIssues); }
        );
    }
}