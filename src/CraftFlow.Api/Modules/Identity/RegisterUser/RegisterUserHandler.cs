using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.RegisterUser
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public RegisterUserHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.ToLowerInvariant();

            var exists = await _dbContext.Users
                .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

            if (exists)
            {
                return Result.Failure<Guid>(Error.Conflict(ErrorCodes.Auth.USER_ALREADY_EXISTS));
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = User.Create(request.TenantId, request.Email, passwordHash, request.FullName);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(user.Id);
        }
    }
}
