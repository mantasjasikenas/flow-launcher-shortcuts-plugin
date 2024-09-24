namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

public interface IPluginManager
{
    /// <summary>
    /// The context of the plugin
    /// </summary>
    PluginInitContext Context { get; }

    /// <summary>
    /// The public API of the plugin
    /// </summary>
    IPublicAPI API { get; }

    /// <summary>
    /// The metadata of the plugin
    /// </summary>
    PluginMetadata Metadata { get; }

    /// <summary>
    /// The last query that was sent to the plugin
    /// </summary>
    Query LastQuery { get; }

    /// <summary>
    /// Set the last query that was sent to the plugin
    /// </summary>
    /// <param name="query">The query to set</param>
    void SetLastQuery(Query query);

    /// <summary>
    /// Used to set the reloadable object (cannot inject it in the constructor because of circular dependency)
    /// </summary>
    /// <param name="reloadable"></param>
    void SetReloadable(IReloadable reloadable);

    /// <summary>
    /// Clear the last query
    /// </summary>
    void ClearLastQuery();

    /// <summary>
    /// Reload the data of the plugin
    /// </summary>
    void ReloadPluginData();

    /// <summary>
    /// Get the action keyword from the last query or the first action keyword from the plugin metadata
    /// </summary>
    /// <returns></returns>
    string GetActionKeyword();

    /// <summary>
    /// Append the action keyword in front of the text
    /// </summary>
    /// <param name="text">The text to append</param>
    /// <returns>The text with the action keyword appended</returns>
    string AppendActionKeyword(string text);

    /// <summary>
    /// Change the query with the appended action keyword in front of the text
    /// </summary>
    /// <param name="text">The text to append</param>
    void ChangeQueryWithAppendedKeyword(string text);
}