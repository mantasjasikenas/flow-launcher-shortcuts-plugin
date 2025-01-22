using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models;

public class ParsedQuery
{
    /// <summary>
    /// Text before the first hyphen
    /// </summary>
    public string FirstTerm { get; set; }
    
    /// <summary>
    /// Arguments in the format -x value with multiple words
    /// </summary>
    public Dictionary<string, string> Arguments { get; set; }
    
    /// <summary>
    /// Query search split by space with proper escaping 
    /// </summary>
    public List<string> CommandArguments { get; set; }
    
    /// <summary>
    /// Original query
    /// </summary>
    public Query Query { get; set; }
}