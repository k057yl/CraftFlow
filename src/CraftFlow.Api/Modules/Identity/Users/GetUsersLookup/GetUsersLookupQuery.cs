using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Users.GetUsersLookup;

public record GetUsersLookupQuery : IRequest<Result<List<LookupDto>>>;