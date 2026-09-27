using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CalculateRequirements;

public sealed record CalculateRequirementsQuery : IRequest<Result<MrpReportDto>>;