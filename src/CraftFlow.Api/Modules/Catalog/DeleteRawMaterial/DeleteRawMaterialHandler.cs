using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.DeleteRawMaterial;

public class DeleteRawMaterialHandler : IRequestHandler<DeleteRawMaterialCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteRawMaterialHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteRawMaterialCommand request, CancellationToken cancellationToken)
    {
        var rawMaterial = await _dbContext.RawMaterials
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rawMaterial is null)
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));

        rawMaterial.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}