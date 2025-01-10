using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class VariablesCommand : ICommand
{
    private readonly IVariablesService _variablesService;

    public VariablesCommand(IVariablesService variablesService)
    {
        _variablesService = variablesService;
    }

    public Command Create()
    {
        return CreateVariablesCommand();
    }

    private Command CreateVariablesCommand()
    {
        var addVariableValueArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable value", "What should your variable value be?"))
            .WithHandler(AddVariableCommandHandler)
            .Build();

        var addVariableNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable name", "How should your variable be named?"))
            .WithArgument(addVariableValueArgument)
            .Build();

        var addVariable = new ArgumentLiteralBuilder()
            .WithKey("add")
            .WithResponseInfo(("var add", "Add variable"))
            .WithResponseFailure(("Failed to add variable", "Something went wrong"))
            .WithArgument(addVariableNameArgument)
            .Build();

        var removeVariableArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter variable name", "Which variable should be removed?"))
            .WithResponseSuccess(("Remove", "Your variable will be removed from the list"))
            .WithHandler(RemoveVariableCommandHandler)
            .Build();

        var removeVariable = new ArgumentLiteralBuilder()
            .WithKey("remove")
            .WithResponseInfo(("var remove", "Remove variable"))
            .WithResponseFailure(("Failed to remove variable", "Something went wrong"))
            .WithArgument(removeVariableArgument)
            .Build();

        var listVariables = new ArgumentLiteralBuilder()
            .WithKey("list")
            .WithResponseInfo(("var list", "List all variables"))
            .WithResponseFailure(("Failed to list variables", "Something went wrong"))
            .WithResponseSuccess(("List", "List all variables"))
            .WithHandler(ListVariablesCommandHandler)
            .Build();

        return new CommandBuilder()
            .WithKey("var")
            .WithResponseInfo(("var", "Manage variables"))
            .WithResponseFailure(("Failed to manage variables", "Something went wrong"))
            .WithArguments(addVariable, removeVariable, listVariables)
            .Build();
    }

    private List<Result> ListVariablesCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.GetVariablesList();
    }

    private List<Result> RemoveVariableCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.RemoveVariable(arguments[2]);
    }

    private List<Result> AddVariableCommandHandler(ActionContext context, List<string> arguments)
    {
        return _variablesService.AddVariable(arguments[2], arguments[3]);
    }
}