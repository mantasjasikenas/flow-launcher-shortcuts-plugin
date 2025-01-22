using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class QueryParser : IQueryParser
{
    public ParsedQuery Parse(Query query)
    {
        var textUntilHyphen = RegexPattern.TextUntilHyphenOrEndLinePattern().Match(query.Search).Value.Trim();
        var argumentsDictionary = MatchArguments(query.Search);
        var arguments = SplitCommandArguments(query.Search);    

        return new ParsedQuery
        {
            FirstTerm = textUntilHyphen,
            Arguments = argumentsDictionary,
            Query = query,
            CommandArguments = arguments
        };
    }

    private static Dictionary<string, string> MatchArguments(string query)
    {
        var arguments = new Dictionary<string, string>();

        var matches = RegexPattern.ArgumentNameValuePattern().Matches(query);

        foreach (Match match in matches)
        {
            var argumentName = match.Groups[1].Value;
            var argumentValue = match.Groups[2].Value.Trim();

            if (!string.IsNullOrEmpty(argumentValue))
            {
                arguments[argumentName] = argumentValue;
            }
        }

        return arguments;
    }

    private static List<string> SplitCommandArguments(string commandLine)
    {
        var arguments = new List<string>();

        var inQuotes = false;
        var isEscaped = false;
        var argStartIndex = 0;

        for (var i = 0; i < commandLine.Length; i++)
        {
            var currentChar = commandLine[i];

            switch (currentChar)
            {
                case '\\' when !isEscaped:
                    isEscaped = true;
                    continue;
                case '\"':
                    if (isEscaped)
                    {
                        isEscaped = false; // reset the flag if the quote is escaped
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                case ' ' when !inQuotes:
                {
                    if (i > argStartIndex)
                    {
                        var argument = commandLine.Substring(argStartIndex, i - argStartIndex);

                        arguments.Add(argument);
                    }

                    argStartIndex = i + 1;
                    break;
                }
            }

            if (currentChar != '\\')
            {
                isEscaped = false; // reset the flag if the current character is not a backslash
            }
        }

        if (commandLine.Length > argStartIndex)
        {
            var lastArgument = commandLine[argStartIndex..];
            arguments.Add(lastArgument);
        }

        arguments = arguments
                    .Select(x =>
                        {
                            var input = x.Replace("\\\"", "\"");

                            return input.StartsWith("\"") && input.EndsWith("\"") && input.Length > 1
                                ? input[1..^1]
                                : input;
                        }
                    )
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

        return arguments;
    }
}