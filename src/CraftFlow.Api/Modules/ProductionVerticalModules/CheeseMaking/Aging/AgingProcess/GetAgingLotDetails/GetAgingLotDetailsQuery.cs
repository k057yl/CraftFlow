using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.GetAgingLotDetails;

public record GetAgingLotDetailsQuery(Guid LotId) : IRequest<Result<GetAgingLotDetailsDto>>;