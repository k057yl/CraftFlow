using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement.Suppliers;

public sealed record CreateSupplierCommand(string Name, string? Phone, string? Email) : IRequest<Result<Guid>>;
