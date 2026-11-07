using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Auth.ResendOtp;

public record ResendOtp(string Email) : IRequest<Result<bool>>;