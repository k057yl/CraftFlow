using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Infrastructure.Services;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.ResendOtp;
public class ResendOtpHandler : IRequestHandler<ResendOtpCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;

    public ResendOtpHandler(AppDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task<Result<bool>> Handle(ResendOtpCommand request, CancellationToken cancellationToken)
    {
        var sanitizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == sanitizedEmail, cancellationToken);

        if (user == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Auth.USER_NOT_FOUND));
        }

        if (user.IsActive)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.ALREADY_ACTIVATED));
        }

        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);

        user.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(5));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _emailService.SendOtpCodeAsync(user.Email, rawOtpCode);

        return Result.Success(true);
    }
}