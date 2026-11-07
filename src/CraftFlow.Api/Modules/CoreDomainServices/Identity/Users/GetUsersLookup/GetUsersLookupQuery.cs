using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.GetUsersLookup;

public record GetUsersLookupQuery : IRequest<Result<List<LookupDto>>>;