using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.GetOrganizations;

public record GetOrganizationsQuery() : IRequest<Result<List<OrganizationAdminDto>>>;