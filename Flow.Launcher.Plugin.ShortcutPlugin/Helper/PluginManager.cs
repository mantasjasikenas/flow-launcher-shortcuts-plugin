using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class PluginManager : IPluginManager
{
    public PluginInitContext Context { get; }

    public IPublicAPI API => Context.API;

    public PluginMetadata Metadata => Context.CurrentPluginMetadata;

    public Query LastQuery { get; private set; }


    private IReloadable _reloadable;


    public PluginManager(PluginInitContext context)
    {
        Context = context;
        LastQuery = new Query();
    }

    public void SetReloadable(IReloadable reloadable)
    {
        _reloadable = reloadable;
    }

    public void SetLastQuery(Query query)
    {
        LastQuery = query;
    }

    public void ClearLastQuery()
    {
        LastQuery = new Query();
    }

    public void ReloadPluginData()
    {
        ClearLastQuery();
        _reloadable.ReloadData();
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