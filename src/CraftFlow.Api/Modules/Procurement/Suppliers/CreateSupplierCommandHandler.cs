using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Procurement.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement.Suppliers;

public sealed class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateSupplierCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(request.Name, request.Phone, request.Email);

        await _dbContext.Set<Supplier>().AddAsync(supplier, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supplier.Id);
    }
}