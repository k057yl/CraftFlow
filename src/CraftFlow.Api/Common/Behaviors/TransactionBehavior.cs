using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
    {
        private readonly AppDbContext _dbContext;

        public TransactionBehavior(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!typeof(TRequest).Name.EndsWith("Command"))
            {
                return await next();
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async ct =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

                var response = await next();

                if (response.IsSuccess)
                {
                    await _dbContext.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }

                return response;
            }, cancellationToken);
        }
    }
}
