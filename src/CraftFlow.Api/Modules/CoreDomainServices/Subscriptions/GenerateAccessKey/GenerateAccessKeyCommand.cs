using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GenerateAccessKey;
public record GenerateAccessKeyCommand(string KeyName) : IRequest<Result<GenerateAccessKeyResponse>>;