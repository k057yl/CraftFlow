using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.DeleteUnitOfMeasure;

public record DeleteUnitOfMeasureCommand(Guid Id) : IRequest<Result>;