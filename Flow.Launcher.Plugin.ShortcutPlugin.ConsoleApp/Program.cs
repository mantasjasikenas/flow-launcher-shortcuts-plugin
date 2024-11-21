using Flow.Launcher.Plugin.ShortcutPlugin.ConsoleApp;


var schema = JsonSchema.GenerateShortcutSchema();

var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
var path = Path.Combine(desktop, "shortcuts-schema.json");

File.WriteAllText(path, schema);