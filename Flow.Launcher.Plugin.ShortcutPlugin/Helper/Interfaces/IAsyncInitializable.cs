using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;

public interface IAsyncInitializable
{
    Task InitializeAsync();
}