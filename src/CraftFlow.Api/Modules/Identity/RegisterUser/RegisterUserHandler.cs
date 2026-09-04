using System.Text.RegularExpressions;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public RegisterUserHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(user.Id);
    }
}