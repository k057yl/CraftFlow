using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.SharedKernel.Result;
using CraftFlow.SharedKernel.Security;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.VerifyOtp;

public record VerifyOtpCommand(
    string Email,
    [property: SkipSanitization] string OtpCode,
    bool RememberMe = false
) : IRequest<Result<LoginResponseDto>>;