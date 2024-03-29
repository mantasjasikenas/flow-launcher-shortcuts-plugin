﻿using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class VariablesService : IVariablesService
{
    private readonly IVariablesRepository _variablesRepository;

    public VariablesService(IVariablesRepository variablesRepository)
    {
        _variablesRepository = variablesRepository;
    }

    public List<Result> GetVariables()
    {
        var variables = _variablesRepository.GetVariables();

        if (variables.Count == 0)
            return ResultExtensions.EmptyResult("No variables found.");

        return variables
            .Select(variable => new Result
            {
                Title = $"Variable '{variable.Name}'",
                SubTitle = $"Value: '{variable.Value}'",
                IcoPath = Icons.Logo,
                Action = _ => true
            })
            .ToList();
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
            () =>
            {
                _variablesRepository.AddVariable(name, value);
            }
        );
    }

    public List<Result> RemoveVariable(string name)
    {
        var variable = _variablesRepository.GetVariable(name);

        if (variable is null)
            return ResultExtensions.EmptyResult($"Variable '{name}' not found.");

        return ResultExtensions.SingleResult(
            $"Remove variable '{name}'",
            $"Value: '{variable.Value}'",
            () =>
            {
                _variablesRepository.RemoveVariable(name);
            }
        );
    }

    public List<Result> UpdateVariable(string name, string value)
    {
        var variable = _variablesRepository.GetVariable(name);

        if (variable is null)
            return ResultExtensions.EmptyResult($"Variable '{name}' not found.");

        return ResultExtensions.SingleResult(
            $"Update variable '{name}'",
            $"Old value '{variable.Value}' | New value '{value}'",
            () =>
            {
                _variablesRepository.UpdateVariable(name, value);
            }
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
