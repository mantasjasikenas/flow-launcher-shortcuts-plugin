using System.Text.RegularExpressions;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public static partial class RegexPattern
{
    /*
     * Match ${argument}
     * Group 1 => argument
     */
    [GeneratedRegex(@"\$\{(.*?)\}")]
    public static partial Regex ArgumentTemplatePattern();

    /*
     * Match x => -x value with multiple words
     * Group 1 => argument name
     * Group 2 => argument value
     */
    [GeneratedRegex("""-(\w+)(\s+(?:(?:[^-].*?|\".*?\"))+)+""")]
    public static partial Regex ArgumentNameValuePattern();


    /*
     * Match text from line start until hyphen
     */
    [GeneratedRegex(@"^[^-]*")]
    public static partial Regex TextUntilHyphenOrEndLinePattern();
}