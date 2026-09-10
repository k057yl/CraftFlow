using CraftFlow.Api.Common.Infrastructure.Identity;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public LoginUserHandler(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.ACCOUNT_NOT_ACTIVATED));
        }

        var tokenString = _tokenService.GenerateJwtToken(user);

        return Result.Success(new LoginResponseDto(tokenString, user.TenantId, user.FullName, user.Email, user.IsAdmin));
    }
}