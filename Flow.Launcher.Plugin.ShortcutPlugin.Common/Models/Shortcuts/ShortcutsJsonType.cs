using System.ComponentModel;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public record ShortcutsJsonType([property: Description("Shortcuts list")] List<Shortcut> Shortcuts);
