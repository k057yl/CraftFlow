using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetAccessKeys;
public record GetAccessKeysQuery : IRequest<Result<List<AccessKeyDto>>>;