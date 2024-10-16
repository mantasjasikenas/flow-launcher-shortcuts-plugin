using System.Collections.ObjectModel;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public abstract partial class ObservableShortcut : ObservableValidator
{
    private readonly Shortcut _shortcut;
    private readonly ObservableCollection<string> _alias;

    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;


    public ObservableShortcut(Shortcut shortcut)
    {
        _shortcut = shortcut;
        _alias = new ObservableCollection<string>(shortcut.Alias ?? []);
    }


    [Required(ErrorMessage = "Shortcut key is required")]
    [MinLength(1, ErrorMessage = "Shortcut key must be at least 1 character long")]
    public string Key
    {
        get => _shortcut.Key;
        set => SetProperty(_shortcut.Key, value, _shortcut, (u, n) => u.Key = n);
    }

    public string Description
    {
        get => _shortcut.Description;
        set => SetProperty(_shortcut.Description, value, _shortcut, (u, n) => u.Description = n);
    }

    public string Icon
    {
        get => _shortcut.Icon;
        set => SetProperty(_shortcut.Icon, value, _shortcut, (u, n) => u.Icon = n);
    }

    [CustomValidation(typeof(ObservableShortcut), nameof(ValidateAlias))]
    public ObservableCollection<string> Alias
    {
        get => _alias;
        set => SetProperty(_alias, value, _shortcut, (u, n) => u.Alias = new List<string>(n));
    }

    [ObservableProperty]
    private string errors;

    [RelayCommand]
    private void Submit()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            ShowErrors();
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ShowErrors()
    {
        Errors = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
    }

    public string GetDerivedType() => _shortcut.GetDerivedType();

    public string GetTitle() => _shortcut.GetTitle();

    public string GetSubTitle() => _shortcut.GetSubTitle();

    public Shortcut Clone()
    {
        return (Shortcut)_shortcut.Clone();
    }

    public virtual Shortcut GetBaseShortcut()
    {
        _shortcut.Alias = new List<string>(_alias);

        return _shortcut;
    }

    public static ValidationResult ValidateAlias(ObservableCollection<string> alias)
    {
        if (alias.Any(string.IsNullOrWhiteSpace))
        {
            return new("Alias cannot be empty text", [nameof(Alias)]);
        }

        return ValidationResult.Success;
    }
}
