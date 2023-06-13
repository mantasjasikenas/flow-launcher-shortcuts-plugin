using System;
using System.Linq;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utils;

public static class QueryBuilder
{
    public static Query Build(string rawQuery)
    {
        if (string.IsNullOrWhiteSpace(rawQuery))
        {
            return null;
        }

        var terms = rawQuery.Split(Query.TermSeparator, StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0)
        {
            return null;
        }


        var search = terms.ElementAtOrDefault(1);
        var searchTerms = terms[1..];
        var actionKeyword = terms.FirstOrDefault();


        var query = new Query(rawQuery, search, terms, searchTerms, actionKeyword);

        return query;
    }
}