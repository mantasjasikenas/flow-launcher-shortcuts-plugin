using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static partial class CommandLineExtensions
{
    [GeneratedRegex("\\$\\{(.*?)\\}")]
    private static partial Regex ArgumentRegex();

    public static List<string> SplitArguments(string commandLine)
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
                case '\"' when !isEscaped:
                    inQuotes = !inQuotes;
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

            isEscaped = false;
        }

        if (commandLine.Length <= argStartIndex) return arguments;

        var lastArgument = commandLine[argStartIndex..];
        arguments.Add(lastArgument);

        arguments = arguments.Select(x => x.Replace("\"", "")).Where(x => !string.IsNullOrEmpty(x)).ToList();

        return arguments;
    }

    public static Dictionary<string, string> ParseArguments(string shortcut, IReadOnlyList<string> args)
    {
        var arguments = new Dictionary<string, string>();

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith("-")) continue;

            var value = string.Empty;
            while (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
            {
                value += args[++i] + " ";
            }

            arguments.Add(arg, value.Trim());
        }

        // remove empty arguments or where key is -
        arguments = arguments.Where(x => !string.IsNullOrEmpty(x.Key) && x.Key != "-" &&
                                         !string.IsNullOrEmpty(x.Value))
                             .ToDictionary(x => x.Key, x => x.Value);

        // if no arguments found, map all arguments to the first argument
        var matches = ArgumentRegex().Matches(shortcut);

        if (arguments.Count != 0 || matches.Count < 1)
        {
            return arguments;
        }

        var argumentName = matches[0].Groups[1].Value;

        if (!args.Contains($"-{argumentName}"))
        {
            arguments.Add(argumentName, string.Join(" ", args));
        }


        return arguments;
    }
}