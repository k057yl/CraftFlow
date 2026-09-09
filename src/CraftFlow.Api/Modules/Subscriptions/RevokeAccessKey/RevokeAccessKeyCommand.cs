using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Subscriptions.RevokeAccessKey;

public record RevokeAccessKeyCommand(Guid KeyId) : IRequest<Result>;