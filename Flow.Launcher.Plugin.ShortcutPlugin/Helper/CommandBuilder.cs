using System;
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class CommandBuilder : BaseQueryBuilder<Command>;

public class ArgumentBuilder : BaseQueryBuilder<Argument>;

public class ArgumentLiteralBuilder : BaseQueryBuilder<ArgumentLiteral>;

public abstract class BaseQueryBuilder<T> where T : BaseQueryExecutor, new()
{
    private readonly T _instance = new();

    public BaseQueryBuilder<T> WithKey(string key)
    {
        _instance.Key = key;
        return this;
    }

    public BaseQueryBuilder<T> WithMultipleValuesForSingleArgument(bool value = true)
    {
        _instance.AllowsMultipleValuesForSingleArgument = value;
        return this;
    }

    public BaseQueryBuilder<T> WithResponseInfo((string, string) responseInfo)
    {
        _instance.ResponseInfo = responseInfo;
        return this;
    }

    public BaseQueryBuilder<T> WithResponseFailure((string, string) responseFailure)
    {
        _instance.ResponseFailure = responseFailure;
        return this;
    }

    public BaseQueryBuilder<T> WithResponseSuccess((string, string) responseSuccess)
    {
        _instance.ResponseSuccess = responseSuccess;
        return this;
    }

    public BaseQueryBuilder<T> WithHandler(Func<ActionContext, List<string>, List<Result>> handler)
    {
        _instance.Handler = handler;
        return this;
    }

    public BaseQueryBuilder<T> WithArgument(IQueryExecutor argument)
    {
        _instance.Arguments ??= [];
        _instance.Arguments.Add(argument);

        return this;
    }

    public BaseQueryBuilder<T> WithArguments(IEnumerable<IQueryExecutor> arguments)
    {
        _instance.Arguments ??= [];
        _instance.Arguments.AddRange(arguments);

        return this;
    }

    public BaseQueryBuilder<T> WithArguments(params IQueryExecutor[] arguments)
    {
        _instance.Arguments ??= [];
        _instance.Arguments.AddRange(arguments);

        return this;
    }

    public T Build()
    {
        return _instance;
    }
}