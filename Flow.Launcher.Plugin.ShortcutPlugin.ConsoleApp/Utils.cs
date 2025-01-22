namespace Flow.Launcher.Plugin.ShortcutPlugin.ConsoleApp;

public static class Utils
{
    public static void GenerateShortcutsSchemaToDesktop()
    {
        var schema = JsonSchema.GenerateShortcutSchema();

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, "shortcuts-schema.json");

        File.WriteAllText(path, schema);
    }
}