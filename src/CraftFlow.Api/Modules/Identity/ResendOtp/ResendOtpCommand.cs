using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.ResendOtp;

public record ResendOtpCommand(string Email) : IRequest<Result<bool>>;