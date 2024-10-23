using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class VariablesViewModel : ObservableRecipient, INavigationAware
{
    private readonly IVariablesService _variablesService;
    private readonly INavigationService _navigationService;

    private IEnumerable<Variable> Variables = [];

    [ObservableProperty]
    private string foundVariablesTitle;

    [ObservableProperty]
    private string lastUpdated;

    public AutoSuggestBox AutoSuggestBox
    {
        get;
        set;
    }

    public ObservableCollection<Variable> FilteredVariables { get; private set; } = [];

    public IAsyncRelayCommand LoadVariablesCommand
    {
        get;
    }


    public VariablesViewModel(IVariablesService variablesService, INavigationService navigationService)
    {
        _variablesService = variablesService;
        _navigationService = navigationService;

        LoadVariablesCommand = new AsyncRelayCommand(LoadVariablesAsync);
    }

    public void NavigateToVariableDetails(Variable variable, bool isEditEnabled)
    {
        _navigationService.NavigateTo(typeof(VariableDetailsViewModel).ToString(), new VariableDetailsNavArgs(variable, DetailsPageMode.Edit, isEditEnabled));
    }

    public async Task DeleteVariableAsync(Variable variable)
    {
        await _variablesService.DeleteVariableAsync(variable);
        await LoadVariablesCommand.ExecuteAsync(null);
    }

    public void OnNewVariableClicked()
    {
        _navigationService.NavigateTo(typeof(VariableDetailsViewModel).ToString(), new VariableDetailsNavArgs(new Variable(), DetailsPageMode.New, true));
    }

    public async Task OnNavigatedTo(object parameter)
    {
        await LoadVariablesCommand.ExecuteAsync(null);
    }

    private void ClearSearchBox()
    {
        AutoSuggestBox.Text = string.Empty;
    }

    private async Task LoadVariablesAsync()
    {
        ClearSearchBox();
        FilteredVariables.Clear();

        await _variablesService.RefreshVariablesAsync();
        Variables = await _variablesService.GetVariablesAsync();

        LastUpdated = "Last updated: " + DateTime.Now.ToString("HH:mm:ss");

        foreach (var variable in Variables)
        {
            FilteredVariables.Add(variable);
        }

        FoundVariablesTitle = GenerateVariablesTitle();
    }

    public Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }

    public void OnFilterChanged(string filter)
    {
        var filtered = Variables.Where(variable => Filter(variable, filter));

        RemoveNonMatchingVariables(filtered);
        AddMatchingVariables(filtered);

        FoundVariablesTitle = GenerateVariablesTitle();
    }

    private string GenerateVariablesTitle()
    {
        var count = FilteredVariables.Count;

        if (count == 0)
        {
            return "We couldn't find any variables";
        }

        return $"{count} variable{(count > 1 ? "s" : string.Empty)} found";
    }

    private static bool Filter(Variable variable, string query)
    {
        return variable.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            variable.Value.Contains(query, StringComparison.InvariantCultureIgnoreCase);
    }

    private void RemoveNonMatchingVariables(IEnumerable<Variable> filteredData)
    {
        for (var i = FilteredVariables.Count - 1; i >= 0; i--)
        {
            var item = FilteredVariables[i];

            if (!filteredData.Contains(item))
            {
                FilteredVariables.Remove(item);
            }
        }
    }

    private void AddMatchingVariables(IEnumerable<Variable> filteredData)
    {
        foreach (var item in filteredData)
        {
            if (!FilteredVariables.Contains(item))
            {
                FilteredVariables.Add(item);
            }
        }
    }
}
