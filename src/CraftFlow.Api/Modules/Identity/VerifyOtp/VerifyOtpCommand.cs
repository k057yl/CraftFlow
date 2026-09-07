using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.VerifyOtp;

public record VerifyOtpCommand(string Email, string OtpCode) : IRequest<Result<LoginResponseDto>>;