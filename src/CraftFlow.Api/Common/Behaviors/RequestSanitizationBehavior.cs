using System.Reflection;
using CraftFlow.SharedKernel.Security;
using MediatR;

namespace CraftFlow.Api.Common.Behaviors;

public sealed class RequestSanitizationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);

        if (requestType.Namespace != null && requestType.Namespace.Contains("Identity"))
        {
            return await next();
        }

        SanitizeObject(request);
        return await next();
    }

    private static void SanitizeObject(object? obj)
    {
        if (obj is null) return;

        var type = obj.GetType();

        if (type.IsValueType || type == typeof(string) || type.Assembly.IsDynamic)
            return;

        if (obj is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                SanitizeObject(item);
            }
            return;
        }

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                var currentValue = (string?)property.GetValue(obj);
                if (!string.IsNullOrEmpty(currentValue))
                {
                    var cleanValue = InputSanitizer.Sanitize(currentValue);

                    if (property.CanWrite)
                    {
                        property.SetValue(obj, cleanValue);
                    }
                    else
                    {
                        var backingField = type.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                        backingField?.SetValue(obj, cleanValue);
                    }
                }
            }
            else if (property.PropertyType.IsClass)
            {
                var nestedObject = property.GetValue(obj);
                SanitizeObject(nestedObject);
            }
        }
    }
}