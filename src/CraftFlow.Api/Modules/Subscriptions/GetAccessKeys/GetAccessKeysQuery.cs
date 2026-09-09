using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Subscriptions.GetAccessKeys;
public record GetAccessKeysQuery : IRequest<Result<List<AccessKeyDto>>>;