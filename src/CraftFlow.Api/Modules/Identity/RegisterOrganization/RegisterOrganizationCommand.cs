using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.RegisterOrganization;
public record RegisterOrganizationCommand(
    string CompanyName,
    string OwnerEmail,
    string OwnerPassword,
    string OwnerFullName
) : IRequest<Result<Guid>>;