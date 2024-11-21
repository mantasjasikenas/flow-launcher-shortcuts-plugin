using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models;

public partial class ObservableVariable : ObservableValidator
{
    private readonly Variable _variable;

    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;


    public ObservableVariable(Variable variable)
    {
        _variable = variable;
    }


    [Required(ErrorMessage = "Variable name is required")]
    [MinLength(1, ErrorMessage = "Variable name must be at least 1 character long")]
    public string Name
    {
        get => _variable.Name;
        set => SetProperty(_variable.Name, value, _variable, (u, n) => u.Name = n);
    }

    [Required(ErrorMessage = "Variable value is required")]
    [MinLength(1, ErrorMessage = "Variable value must be at least 1 character long")]
    public string Value
    {
        get => _variable.Value;
        set => SetProperty(_variable.Value, value, _variable, (u, n) => u.Value = n);
    }

    [ObservableProperty]
    public partial string Errors
    {
        get; set;
    }

    public bool Validate()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            ShowErrors();
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);

            return false;
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);

            return true;
        }
    }

    private void ShowErrors()
    {
        Errors = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));
    }

    public Variable Clone()
    {
        return new Variable
        {
            Name = Name,
            Value = Value
        };
    }

    public virtual Variable GetBaseVariable()
    {
        return _variable;
    }
}
