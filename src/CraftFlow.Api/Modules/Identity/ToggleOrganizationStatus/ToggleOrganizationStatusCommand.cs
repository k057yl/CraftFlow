using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.ToggleOrganizationStatus;

public record ToggleOrganizationStatusCommand(Guid OrganizationId) : IRequest<Result<bool>>;