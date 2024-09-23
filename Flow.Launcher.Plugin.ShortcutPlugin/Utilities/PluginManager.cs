namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public class PluginManager : IPluginManager
{
    public PluginInitContext Context { get; }

    public IPublicAPI API => Context.API;

    public PluginMetadata Metadata => Context.CurrentPluginMetadata;

    public Query LastQuery { get; private set; }


    public PluginManager(PluginInitContext context)
    {
        Context = context;
        LastQuery = new Query();
    }

    public void SetLastQuery(Query query)
    {
        LastQuery = query;
    }

    public void ClearLastQuery()
    {
        LastQuery = new Query();
    }

    public string GetActionKeyword()
    {
        return LastQuery == null
            ? Context.CurrentPluginMetadata.ActionKeywords[0]
            : LastQuery.ActionKeyword;
    }

    public string AppendActionKeyword(string text)
    {
        var actionKeyword = GetActionKeyword();

        return string.IsNullOrWhiteSpace(actionKeyword)
            ? text
            : $"{actionKeyword} {text}";
    }

    public void ChangeQueryWithAppendedKeyword(string text)
    {
        var query = AppendActionKeyword(text);

        API.ChangeQuery(query);
    }
}