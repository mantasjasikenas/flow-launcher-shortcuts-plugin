using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models;

public class ParsedQuery
{
    /// <summary>
    /// Text before the first hyphen
    /// </summary>
    public required string FirstTerm
    {
        get;
        init;
    }

    /// <summary>
    /// Arguments in the format -x value with multiple words
    /// </summary>
    public required Dictionary<string, string> Arguments
    {
        get;
        init;
    }

    /// <summary>
    /// Query search split by space with proper escaping 
    /// </summary>
    public required List<string> CommandArguments
    {
        get;
        init;
    }

    /// <summary>
    /// Original query
    /// </summary>
    public required Query Query
    {
        get;
        init;
    }
}