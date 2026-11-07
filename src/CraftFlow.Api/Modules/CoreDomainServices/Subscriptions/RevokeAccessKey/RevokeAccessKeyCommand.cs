using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RevokeAccessKey;

public record RevokeAccessKeyCommand(Guid KeyId) : IRequest<Result>;