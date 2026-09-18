using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Infrastructure.Services;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;

public class CreateTenantUserHandler : IRequestHandler<CreateTenantUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IEmailService _emailService;

    public CreateTenantUserHandler(AppDbContext dbContext, ITenantContext tenantContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(CreateTenantUserCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<Guid>(Error.Validation("ONLY_OWNER_CAN_CREATE_USERS"));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.Auth.USER_ALREADY_EXISTS));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(_tenantContext.TenantId, normalizedEmail, passwordHash, request.FullName, request.Role);
        var rawOtpCode = new Random().Next(100000, 999999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(rawOtpCode);
        user.SetOtpCode(otpHash, DateTime.UtcNow.AddMinutes(10));

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _emailService.SendOtpCodeAsync(user.Email, rawOtpCode);

        return Result.Success(user.Id);
    }
}