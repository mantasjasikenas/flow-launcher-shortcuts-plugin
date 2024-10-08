﻿using System;
using System.Collections.Generic;
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

    public static List<Result> EmptyResult(string title, string subtitle = "")
    {
        return SingleResult(title, subtitle);
    }

    public static List<Result> SingleResult(
        string title,
        string subtitle = "",
        Action action = default,
        bool hideAfterAction = true,
        string autocomplete = default,
        string iconPath = default,
        IList<int> titleHighlightData = default
    )
    {
        return
        [
            new Result
            {
                Title = title,
                SubTitle = subtitle,
                IcoPath = iconPath ?? Icons.Logo,
                AutoCompleteText = autocomplete,
                TitleHighlightData = titleHighlightData,
                Action = _ =>
                {
                    action?.Invoke();
                    return hideAfterAction;
                }
            }
        ];
    }

    public static Result Result(
        string title,
        string subtitle = "",
        Action action = default,
        bool hideAfterAction = true,
        string iconPath = default,
        object contextData = default,
        string autoCompleteText = null,
        IList<int> titleHighlightData = default,
        int score = default,
        string previewFilePath = null
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
            Action = _ =>
            {
                action?.Invoke();
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