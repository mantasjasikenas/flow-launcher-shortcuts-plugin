namespace Flow.Launcher.Plugin.ShortcutPlugin.Test.Helpers;

public static class QueryExtensions
{
    public static Query BuildQuery(string rawQuery)
    {
        var split = rawQuery.Split(" ");

        var actionKeyword = split[0];
        var search = string.Join(" ", split.Skip(1));
        var searchTerms = search.Split(" ");
        var terms = Array.Empty<string>();

#pragma warning disable 618
        return new Query(
            rawQuery,
            search,
            terms,
            searchTerms,
            actionKeyword
        );
    }
}