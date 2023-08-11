using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Extensions;

public static class CommandLineExtensions
{
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

    public static Dictionary<string, string> ParseArguments(IReadOnlyList<string> args)
    {
        var arguments = new Dictionary<string, string>();

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith("-")) continue;

            if (i + 1 < args.Count && !args[i + 1].StartsWith("-"))
            {
                arguments.Add(arg, args[i + 1]);
                i++;
            }
            else
            {
                arguments.Add(arg, string.Empty);
            }
        }

        return arguments;
    }
}