using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class VariableDetailsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IVariablesService _variablesService;
    private readonly INavigationService _navigationService;

    private Variable _variable;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private ObservableVariable variable;

    public DetailsPageMode Mode
    {
        get; private set;
    }

    public VariableDetailsViewModel(IVariablesService variablesService, INavigationService navigationService)
    {
        _variablesService = variablesService;
        _navigationService = navigationService;
    }

    public Task OnNavigatedFrom() => Task.CompletedTask;

    public Task OnNavigatedTo(object parameter)
    {
        var args = (VariableDetailsNavArgs)parameter;

        _variable = args.Variable;
        Mode = args.Mode;
        IsEditMode = args.IsEditEnabled;

        Variable = (_variable with { }).ToObservableVariable();

        return Task.CompletedTask;
    }

    public void NavigateBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private async Task SaveButton()
    {
        if (!Variable.Validate())
        {
            return;
        }

        if (Mode == DetailsPageMode.New)
        {
            await _variablesService.SaveVariableAsync(Variable.ToVariable());
            NavigateBack();
        }
        else if (Mode == DetailsPageMode.Edit)
        {
            IsEditMode = false;
            await _variablesService.UpdateVariableAsync(_variable, Variable.ToVariable());
        }
    }

    [RelayCommand]
    private void DiscardButton()
    {
        if (Mode == DetailsPageMode.New)
        {
            NavigateBack();
        }
        else if (Mode == DetailsPageMode.Edit)
        {
            Variable = (_variable with { }).ToObservableVariable();
            IsEditMode = false;
        }
    }
}

public record VariableDetailsNavArgs(Variable Variable, DetailsPageMode Mode, bool IsEditEnabled);
