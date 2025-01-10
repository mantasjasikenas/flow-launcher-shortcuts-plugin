using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class PluginManager : IPluginManager, IAsyncReloadable
{
    public PluginInitContext Context
    {
        get;
    }

    public IPublicAPI API => Context.API;

    public PluginMetadata Metadata => Context.CurrentPluginMetadata;

    public Query LastQuery
    {
        get;
        private set;
    }

    private IAsyncReloadable _asyncReloadable;
    private IList<PluginPair> _plugins;


    public PluginManager(PluginInitContext context)
    {
        Context = context;
        LastQuery = new Query();
        _plugins = context.API.GetAllPlugins();
    }

    public void SetReloadable(IAsyncReloadable reloadable)
    {
        _asyncReloadable = reloadable;
    }

    public void SetLastQuery(Query query)
    {
        LastQuery = query;
    }

    public void ClearLastQuery()
    {
        LastQuery = new Query();
    }

    public async Task ReloadDataAsync()
    {
        ReloadData();
        await _asyncReloadable.ReloadDataAsync();
    }

    private void ReloadData()
    {
        ClearLastQuery();
        _plugins = Context.API.GetAllPlugins();
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

    public string FindPluginActionKeyword(string pluginName)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Metadata.Name == pluginName);

        return plugin?.Metadata.ActionKeyword ?? string.Empty;
    }
}