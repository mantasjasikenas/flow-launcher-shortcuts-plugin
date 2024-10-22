using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class VariablesService : IVariablesService
{
    private readonly IVariablesRepository _variablesRepository;

    private readonly IPluginManager _pluginManager;

    public VariablesService(IVariablesRepository variablesRepository, IPluginManager pluginManager)
    {
        _variablesRepository = variablesRepository;
        _pluginManager = pluginManager;
    }

    public List<Result> GetVariables()
    {
        var variables = _variablesRepository.GetVariables();

        if (variables.Count == 0)
        {
            return ResultExtensions.EmptyResult("No variables found.");
        }

        var header = new Result
        {
            Title = "Variables",
            SubTitle = "List of all variables",
            IcoPath = Icons.Logo
        };

        var results =
            variables
                .Select(variable => new Result
                {
                    Title = $"{variable.Name}",
                    SubTitle = $"{variable.Value}",
                    IcoPath = Icons.Logo,
                    Action = _ =>
                    {
                        _pluginManager.API.CopyToClipboard($"Variable: {variable.Name} value: {variable.Value}");
                        return true;
                    },
                    AutoCompleteText = $"Variable: {variable.Name} value: {variable.Value}"
                })
                .ToList();

        results.Insert(0, header);

        return results;
    }

    public List<Result> GetVariable(string name)
    {
        var variable = _variablesRepository.GetVariable(name);

        if (variable == null)
            return ResultExtensions.EmptyResult($"Variable '{name}' not found.");

        return ResultExtensions.SingleResult(
            $"Variable '{variable.Name}'",
            $"Value: '{variable.Value}'"
        );
    }

    public List<Result> AddVariable(string name, string value)
    {
        return ResultExtensions.SingleResult(
            $"Add variable '{name}'",
            $"Value: '{value}'",
            () => { _variablesRepository.AddVariable(name, value); }
        );
    }

    public List<Result> RemoveVariable(string name)
    {
        var variables = _variablesRepository
                        .GetPossibleVariables(name)
                        .ToList();

        if (variables.Count == 0)
        {
            return ResultExtensions.EmptyResult($"Variable {name} not found.");
        }

        return variables.Select(variable =>
                            ResultExtensions.Result(
                                $"Remove variable {variable.Name}",
                                $"Value: {variable.Value}",
                                () => { _variablesRepository.RemoveVariable(name); }
                            )
                        )
                        .ToList();
    }

    public List<Result> UpdateVariable(string name, string value)
    {
        var variable = _variablesRepository.GetVariable(name);

        if (variable is null)
            return ResultExtensions.EmptyResult($"Variable {name} not found.");

        return ResultExtensions.SingleResult(
            $"Update variable {name}",
            $"Old value: {variable.Value} | New value: {value}",
            () => { _variablesRepository.UpdateVariable(name, value); }
        );
    }

    public void Reload()
    {
        _variablesRepository.Reload();
    }

    public string ExpandVariables(string value)
    {
        return _variablesRepository.ExpandVariables(value);
    }

    public string ExpandVariables(string value, IReadOnlyDictionary<string, string> arguments)
    {
        var expandedArguments = ExpandArguments(value, arguments);
        return ExpandVariables(expandedArguments);
    }

    public List<Result> ImportVariables()
    {
        return ResultExtensions.SingleResult(
            Resources.Import_variables,
            "",
            () =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Import_variables,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }

                _variablesRepository.ImportVariables(openFileDialog.FileName);
            }
        );
    }

    public List<Result> ExportVariables()
    {
        return ResultExtensions.SingleResult(
            Resources.Export_variables,
            "",
            () =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Export_shortcuts,
                    FileName = "variables.json",
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var exportPath = dialog.FileName;

                _variablesRepository.ExportVariables(exportPath);
            }
        );
    }

    private static string ExpandArguments(string value, IReadOnlyDictionary<string, string> args)
    {
        foreach (var (key, arg) in args)
        {
            var trimmedKey = key.TrimStart('-');
            var replaceValue = string.Format(Constants.VariableFormat, trimmedKey);

            value = value.Replace(replaceValue, arg);
        }

        return value;
    }
}