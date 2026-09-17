using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.SeedUnitsOfMeasure;
public record SeedUnitsOfMeasureCommand(UomPreset Preset) : IRequest<Result<int>>;