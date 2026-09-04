using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using FluentValidation;
using MediatR;

namespace CraftFlow.Api.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : Result
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);

            var validationFailures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)
                .ToList();

            if (validationFailures.Count != 0)
            {
                var firstFailure = validationFailures.First();

                var errorsSummary = string.Join("; ", validationFailures
                    .Select(f => $"{f.PropertyName}: {f.ErrorMessage} (Code: {f.ErrorCode})"));

                _logger.LogWarning("Validation failed for {RequestName}: {Errors}",
                    typeof(TRequest).Name, errorsSummary);

                var errorCode = firstFailure.ErrorCode ?? ErrorCodes.General.VALUE_REQUIRED;

                var genericArgument = typeof(TResponse).IsGenericType
                    ? typeof(TResponse).GetGenericArguments()[0]
                    : typeof(object);

                var failureMethod = typeof(Result)
                    .GetMethod(
                        nameof(Result.Failure),
                        genericParameterCount: 1,
                        types: new[] { typeof(Error) })!
                    .MakeGenericMethod(genericArgument);

                var error = Error.Validation(errorCode);

                return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
            }

            return await next();
        }
    }
}