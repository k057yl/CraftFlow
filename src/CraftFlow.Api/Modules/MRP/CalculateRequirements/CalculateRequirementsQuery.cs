using CraftFlow.Api.Modules.MRP.Contracts;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CalculateRequirements;

public sealed record CalculateRequirementsQuery : IRequest<Result<MrpReportDto>>;