using CraftFlow.Api.Core.Infrastructure.Notifications.Email;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Organizations.RegisterOrganization;

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
        if (string.IsNullOrWhiteSpace(request.PlanCode))
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var normalizedEmail = request.OwnerEmail.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<Guid>(Error.Validation("EMAIL_ALREADY_EXISTS"));
        }

        var selectedPlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode, cancellationToken);

        if (selectedPlan == null)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Saas.PLAN_NOT_FOUND));
        }

        var organization = Organization.Create(request.CompanyName);
        _dbContext.Organizations.Add(organization);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.OwnerPassword);
        var ownerUser = User.Create(
            email: normalizedEmail,
            passwordHash: passwordHash,
            fullName: request.OwnerFullName,
            requireOtp: true
        );

        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);
        ownerUser.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(10));

        _dbContext.Users.Add(ownerUser);

        organization.AddMember(ownerUser.Id, TenantRole.OWNER);

        var initialSubscription = TenantSubscription.CreateFree(organization.Id, selectedPlan.Id);
        _dbContext.TenantSubscriptions.Add(initialSubscription);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendOtpCodeAsync(ownerUser.Email, rawOtpCode);

        return Result.Success(organization.Id);
    }
}