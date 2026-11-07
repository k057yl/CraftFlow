using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Organizations.RegisterOrganization;

public record RegisterOrganizationCommand(
    string CompanyName,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFullName,
    string? PlanCode = null
) : IRequest<Result<Guid>>;