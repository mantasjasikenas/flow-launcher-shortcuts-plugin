using System.Collections.Specialized;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
