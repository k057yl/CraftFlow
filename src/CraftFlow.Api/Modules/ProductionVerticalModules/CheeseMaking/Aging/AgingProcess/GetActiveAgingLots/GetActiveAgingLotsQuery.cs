using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.GetActiveAgingLots;

public record GetActiveAgingLotsQuery() : IRequest<Result<List<AgingLotSummaryDto>>>;