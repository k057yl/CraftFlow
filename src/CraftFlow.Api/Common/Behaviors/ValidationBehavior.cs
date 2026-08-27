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

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
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
                var errorCode = validationFailures.First().ErrorCode ?? ErrorCodes.General.VALUE_REQUIRED;

                return (TResponse)typeof(Result)
                    .GetMethod(nameof(Result.Failure), new[] { typeof(Error) })!
                    .MakeGenericMethod(typeof(TResponse).IsGenericType ? typeof(TResponse).GetGenericArguments()[0] : typeof(object))
                    .Invoke(null, new object[] { Error.Validation(errorCode) })!;
            }

            return await next();
        }
    }
}
