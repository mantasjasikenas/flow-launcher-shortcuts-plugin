using System.Text.Json;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;

public static class Json
{
    public static async Task<T> DeserializeAsync<T>(string value)
    {
        return await Task.Run<T>(() =>
        {
            return JsonSerializer.Deserialize<T>(value);
        });
    }

    public static async Task<string> SerializeAsync(object value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonSerializer.Serialize(value);
        });
    }
}
