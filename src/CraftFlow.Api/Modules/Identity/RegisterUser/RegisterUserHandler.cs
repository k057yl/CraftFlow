using System.Text.RegularExpressions;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Infrastructure.Services;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;

    public RegisterUserHandler(AppDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var sanitizedEmail = request.Email.Trim().ToLowerInvariant();
        var sanitizedFullName = Regex.Replace(request.FullName.Trim(), @"[<>]", string.Empty);

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == sanitizedEmail, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.Auth.USER_ALREADY_EXISTS));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.TenantId, sanitizedEmail, passwordHash, sanitizedFullName);

        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);

        user.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(5));

        _dbContext.Users.Add(user);

        var hasSubscription = await _dbContext.Set<TenantSubscription>()
            .AnyAsync(s => s.TenantId == request.TenantId, cancellationToken);

        if (!hasSubscription)
        {
            var trialPlan = await _dbContext.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.Code == "TRIAL", cancellationToken);

            if (trialPlan != null)
            {
                var subscription = TenantSubscription.CreateTrial(
                    request.TenantId,
                    trialPlan.Id,
                    DateTime.UtcNow.AddDays(14));

                _dbContext.Set<TenantSubscription>().Add(subscription);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendOtpCodeAsync(user.Email, rawOtpCode);

        return Result.Success(user.Id);
    }
}