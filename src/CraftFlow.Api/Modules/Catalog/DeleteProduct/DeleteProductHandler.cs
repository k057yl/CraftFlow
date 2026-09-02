using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.DeleteProduct;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteProductHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));

        product.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}