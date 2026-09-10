using System.Text.RegularExpressions;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Infrastructure.Services;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.RegisterOrganization;

public class RegisterOrganizationHandler : IRequestHandler<RegisterOrganizationCommand, Result<Guid>>
{
    private const string DEFAULT_TRIAL_PLAN_CODE = "TRIAL";
    private const int DEFAULT_TRIAL_DAYS = 14;
    private const int OTP_EXPIRATION_MINUTES = 5;

    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;

    public RegisterOrganizationHandler(AppDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(RegisterOrganizationCommand request, CancellationToken cancellationToken)
    {
        var sanitizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();
        var sanitizedFullName = Regex.Replace(request.OwnerFullName.Trim(), @"[<>]", string.Empty);
        var sanitizedCompanyName = Regex.Replace(request.CompanyName.Trim(), @"[<>]", string.Empty);

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == sanitizedEmail, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.Auth.USER_ALREADY_EXISTS));
        }

        var organization = Organization.Create(sanitizedCompanyName);
        _dbContext.Organizations.Add(organization);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword);
        var owner = User.Create(organization.Id, sanitizedEmail, passwordHash, sanitizedFullName);

        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);

        owner.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(OTP_EXPIRATION_MINUTES));

        _dbContext.Users.Add(owner);

        var trialPlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == DEFAULT_TRIAL_PLAN_CODE, cancellationToken);

        if (trialPlan != null)
        {
            var subscription = TenantSubscription.CreateTrial(
                organization.Id,
                trialPlan.Id,
                DateTime.UtcNow.AddDays(DEFAULT_TRIAL_DAYS));

            _dbContext.TenantSubscriptions.Add(subscription);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendOtpCodeAsync(owner.Email, rawOtpCode);

        return Result.Success(organization.Id);
    }
}