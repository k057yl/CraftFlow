using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CraftFlow.Api.Modules.Identity.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public LoginUserHandler(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var secretKey = _configuration["Jwt:SecretKey"] ?? "SUPER_SECRET_KEY_CRAFT_FLOW_2026_OLD_SCHULL_MUST_BE_LONG_ENOUGH";
        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("tenant_id", user.TenantId.ToString()),
                new Claim("full_name", user.FullName)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Result.Success(new LoginResponseDto(tokenString, user.TenantId, user.FullName));
    }
}