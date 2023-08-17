using System;
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class ResultExtensions
{
    public static List<Result> InitializedResult()
    {
        return SingleResult(Resources.ShortcutsManager_Init_Plugin_initialized);
    }

    public static List<Result> EmptyResult()
    {
        return SingleResult(Resources.Shortcuts_Query_No_results_found);
    }

    public static List<Result> EmptyResult(string title, string subtitle = "")
    {
        return SingleResult(title, subtitle);
    }

    public static List<Result> SingleResult(string title, string subtitle = "", Action action = default,
        bool hideAfterAction = true)
    {
        return new List<Result>
        {
            new()
            {
                Title = title,
                SubTitle = subtitle,
                IcoPath = Constants.IconPath,
                Action = _ =>
                {
                    action?.Invoke();
                    return hideAfterAction;
                }
            }
        };
    }

    public static Result Result(string title, string subtitle = "", Action action = default,
        bool hideAfterAction = true)
    {
        return new Result
        {
            Title = title,
            SubTitle = subtitle,
            IcoPath = "images\\icon.png",
            Action = _ =>
            {
                action?.Invoke();
                return hideAfterAction;
            }
        };
    }
}