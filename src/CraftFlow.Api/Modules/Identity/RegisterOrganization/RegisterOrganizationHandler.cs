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
    private readonly AppDbContext _dbContext;
    private readonly IEmailService _emailService;

    public RegisterOrganizationHandler(AppDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(RegisterOrganizationCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.Auth.USER_ALREADY_EXISTS));
        }

        var organization = Organization.Create(request.CompanyName);
        _dbContext.Organizations.Add(organization);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword);
        var ownerUser = User.Create(
            tenantId: organization.Id,
            email: normalizedEmail,
            passwordHash: passwordHash,
            fullName: request.OwnerFullName,
            role: TenantRole.Owner
        );

        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);
        ownerUser.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(10));

        _dbContext.Users.Add(ownerUser);

        var freePlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == "FREE", cancellationToken)
            ?? await _dbContext.SubscriptionPlans.FirstOrDefaultAsync(cancellationToken);

        if (freePlan != null)
        {
            var freeSubscription = TenantSubscription.CreateFree(organization.Id, freePlan.Id);
            _dbContext.TenantSubscriptions.Add(freeSubscription);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendOtpCodeAsync(ownerUser.Email, rawOtpCode);

        return Result.Success(organization.Id);
    }
}