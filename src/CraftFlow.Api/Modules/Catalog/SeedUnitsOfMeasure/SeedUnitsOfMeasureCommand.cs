using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.SeedUnitsOfMeasure;

public record SeedUnitsOfMeasureCommand : IRequest<Result<int>>;