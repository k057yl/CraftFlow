using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.GetAgingChambers;
public record GetAgingChambersQuery : IRequest<Result<List<AgingChamberDto>>>;