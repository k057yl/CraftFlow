using CraftFlow.SharedKernel.Dtos.Fermentation;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.GetFermentationBatches;

public record GetFermentationBatchesQuery(bool IncludeCompleted = false) : IRequest<Result<List<FermentationBatchDto>>>;