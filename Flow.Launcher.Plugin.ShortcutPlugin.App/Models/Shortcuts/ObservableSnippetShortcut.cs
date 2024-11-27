using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public partial class ObservableSnippetShortcut : ObservableShortcut
{
    private readonly SnippetShortcut _snippetShortcut;

    public ObservableSnippetShortcut(SnippetShortcut snippetShortcut) : base(snippetShortcut)
    {
        _snippetShortcut = snippetShortcut;
    }

    [Required(ErrorMessage = "Value is required")]
    public string Value
    {
        get => _snippetShortcut.Value;
        set => SetProperty(_snippetShortcut.Value, value, _snippetShortcut, (u, n) => u.Value = n);
    }

    public new ObservableSnippetShortcut Clone()
    {
        return new ObservableSnippetShortcut((SnippetShortcut)_snippetShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();
        return _snippetShortcut;
    }

    public override string ToString()
    {
        return _snippetShortcut.ToString();
    }
}
