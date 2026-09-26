using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateRawMaterial;

public class CreateRawMaterialHandler : IRequestHandler<CreateRawMaterialCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateRawMaterialHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateRawMaterialCommand request, CancellationToken cancellationToken)
    {
        var rawMaterial = RawMaterial.Create(request.Name, request.UnitOfMeasureId);

        _dbContext.RawMaterials.Add(rawMaterial);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(rawMaterial.Id);
    }
}