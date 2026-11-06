using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace CraftFlow.SharedKernel.Result;

public static class ResultFactory
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = new();

    public static TResponse CreateFailure<TResponse>(Error error) where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failureMethod = FailureMethodCache.GetOrAdd(valueType, type =>
        {
            return typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Result.Failure)
                            && m.IsGenericMethod
                            && m.GetParameters().Length == 1
                            && m.GetParameters()[0].ParameterType == typeof(Error))
                .MakeGenericMethod(type);
        });

        return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
    }
}