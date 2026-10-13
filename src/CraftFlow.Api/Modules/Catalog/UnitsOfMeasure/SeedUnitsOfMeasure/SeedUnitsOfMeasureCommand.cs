using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.UnitsOfMeasure.SeedUnitsOfMeasure;

public record SeedUnitsOfMeasureCommand : IRequest<Result<int>>;