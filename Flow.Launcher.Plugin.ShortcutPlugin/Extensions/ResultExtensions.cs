using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class ResultExtensions
{
    public static List<Result> ToList(this Result result)
    {
        return [result];
    }

    public static List<Result> EmptyResult()
    {
        return SingleResult(Resources.Shortcuts_Query_No_results_found);
    }

    public static List<Result> EmptyResult(string title, string? subtitle = "")
    {
        return SingleResult(title, subtitle);
    }

    public static List<Result> SingleResult(
        string title,
        string? subtitle = "",
        Action? action = null,
        Func<Task>? asyncAction = null,
        bool hideAfterAction = true,
        string? iconPath = null,
        object? contextData = null,
        string? autoCompleteText = null,
        IList<int>? titleHighlightData = null,
        int score = 0,
        string? previewFilePath = null
    )
    {
        return
        [
            Result(
                title,
                subtitle,
                action,
                asyncAction,
                hideAfterAction,
                iconPath,
                contextData,
                autoCompleteText,
                titleHighlightData,
                score,
                previewFilePath
            )
        ];
    }

    public static Result Result(
        string title,
        string? subtitle = "",
        Action? action = null,
        Func<Task>? asyncAction = null,
        bool hideAfterAction = true,
        string? iconPath = null,
        object? contextData = null,
        string? autoCompleteText = null,
        IList<int>? titleHighlightData = null,
        int score = 0,
        string? previewFilePath = null
    )
    {
        return new Result
        {
            Title = title,
            SubTitle = subtitle,
            IcoPath = iconPath ?? Icons.Logo,
            TitleHighlightData = titleHighlightData,
            ContextData = contextData,
            Score = score,
            Action = action == null
                ? null
                : _ =>
                {
                    action();
                    return hideAfterAction;
                },
            AsyncAction = asyncAction == null
                ? null
                : async _ =>
                {
                    await asyncAction();
                    return hideAfterAction;
                },
            AutoCompleteText = autoCompleteText ?? title,
            Preview = new Result.PreviewInfo
            {
                FilePath = previewFilePath
            }
        };
    }
}