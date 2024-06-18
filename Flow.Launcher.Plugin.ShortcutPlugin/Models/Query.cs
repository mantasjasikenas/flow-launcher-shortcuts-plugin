using System;
using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public partial class Query : IParsable<Query>
{
    public string Keyword { get; init; }

    public string RawQuery { get; init; }

    public string Args { get; init; }


    public static Query Parse(string s, IFormatProvider provider = null)
    {
        var keyword = KeywordRegex().Match(s).ToString();
        var args = ArgsRegex().Match(s).ToString();


        return new Query
        {
            Keyword = keyword,
            RawQuery = s,
            Args = args
        };
    }

    public static bool TryParse(string s, IFormatProvider provider, out Query result)
    {
        result = Parse(s, provider);
        return result is not null;
    }

    public override string ToString()
    {
        return $"Keyword: {Keyword} Args: {Args}";
    }

    [GeneratedRegex("^\\w+")]
    private static partial Regex KeywordRegex();

    [GeneratedRegex("(?<=\\w+ ).+(?<=\\n|$)")]
    private static partial Regex ArgsRegex();
}