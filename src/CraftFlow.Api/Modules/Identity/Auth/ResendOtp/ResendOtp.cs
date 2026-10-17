using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Auth.ResendOtp;

public record ResendOtp(string Email) : IRequest<Result<bool>>;