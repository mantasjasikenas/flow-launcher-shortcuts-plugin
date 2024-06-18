using Flow.Launcher.Plugin.SharedModels;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Test.Helpers;

internal class PublicApi : IPublicAPI
{
    private readonly string _shortcutsPath;
    private readonly string _variablesPath;

    public PublicApi(string shortcutsPath, string variablesPath)
    {
        _shortcutsPath = shortcutsPath;
        _variablesPath = variablesPath;
    }


    public T LoadSettingJsonStorage<T>() where T : new()
    {
        if (typeof(T) == typeof(Settings))
        {
            return (T) (object) new Settings
            {
                ShortcutsPath = _shortcutsPath,
                VariablesPath = _variablesPath
            };
        }

        return new T();
    }

    public void ChangeQuery(string query, bool requery = false)
    {
        throw new NotImplementedException();
    }

    public void RestartApp()
    {
        throw new NotImplementedException();
    }

    public void ShellRun(string cmd, string filename = "cmd.exe")
    {
        throw new NotImplementedException();
    }

    public void CopyToClipboard(string text, bool directCopy = false, bool showDefaultNotification = true)
    {
        throw new NotImplementedException();
    }

    public void SaveAppAllSettings()
    {
        throw new NotImplementedException();
    }

    public void SavePluginSettings()
    {
        throw new NotImplementedException();
    }

    public Task ReloadAllPluginData()
    {
        throw new NotImplementedException();
    }

    public void CheckForNewUpdate()
    {
        throw new NotImplementedException();
    }

    public void ShowMsgError(string title, string subTitle = "")
    {
        throw new NotImplementedException();
    }

    public void ShowMainWindow()
    {
        throw new NotImplementedException();
    }

    public void HideMainWindow()
    {
        throw new NotImplementedException();
    }

    public bool IsMainWindowVisible()
    {
        throw new NotImplementedException();
    }

    public void ShowMsg(string title, string subTitle = "", string iconPath = "")
    {
        TestContext.Out.WriteLine("Title: " + title);
        TestContext.Out.WriteLine("Subtitle: " + subTitle);
    }

    public void ShowMsg(string title, string subTitle, string iconPath, bool useMainWindowAsOwner = true)
    {
        throw new NotImplementedException();
    }

    public void OpenSettingDialog()
    {
        throw new NotImplementedException();
    }

    public string GetTranslation(string key)
    {
        throw new NotImplementedException();
    }

    public List<PluginPair> GetAllPlugins()
    {
        throw new NotImplementedException();
    }

    public void RegisterGlobalKeyboardCallback(Func<int, int, SpecialKeyState, bool> callback)
    {
        throw new NotImplementedException();
    }

    public void RemoveGlobalKeyboardCallback(Func<int, int, SpecialKeyState, bool> callback)
    {
        throw new NotImplementedException();
    }

    public MatchResult FuzzySearch(string query, string stringToCompare)
    {
        throw new NotImplementedException();
    }

    public Task<string> HttpGetStringAsync(string url, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<Stream> HttpGetStreamAsync(string url, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task HttpDownloadAsync(string url, string filePath, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<string> HttpGetString(string url, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<Stream> HttpGetStream(string url, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task HttpDownload(string url, string filePath, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public void AddActionKeyword(string pluginId, string newActionKeyword)
    {
        throw new NotImplementedException();
    }

    public void RemoveActionKeyword(string pluginId, string oldActionKeyword)
    {
        throw new NotImplementedException();
    }

    public bool ActionKeywordAssigned(string actionKeyword)
    {
        throw new NotImplementedException();
    }

    public void LogDebug(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogInfo(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogWarn(string className, string message, string methodName = "")
    {
        throw new NotImplementedException();
    }

    public void LogException(string className, string message, Exception e, string methodName = "")
    {
        throw new Exception($"[{className}] {message}: {e.Message}");
    }

    public void SaveSettingJsonStorage<T>() where T : new()
    {
        throw new NotImplementedException();
    }

    public void OpenDirectory(string DirectoryPath, string FileNameOrFilePath = null)
    {
        throw new NotImplementedException();
    }

    public void OpenUrl(Uri url, bool? inPrivate = null)
    {
        throw new NotImplementedException();
    }

    public void OpenUrl(string url, bool? inPrivate = null)
    {
        throw new NotImplementedException();
    }

    public void OpenAppUri(Uri appUri)
    {
        throw new NotImplementedException();
    }

    public void OpenAppUri(string appUri)
    {
    }

    public void ToggleGameMode()
    {
    }

    public void SetGameMode(bool value)
    {
    }

    public bool IsGameModeOn()
    {
        return false;
    }

    public void ReQuery(bool reselect = true)
    {
    }

    public event VisibilityChangedEventHandler? VisibilityChanged;
}