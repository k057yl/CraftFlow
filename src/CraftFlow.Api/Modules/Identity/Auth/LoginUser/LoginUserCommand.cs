using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Auth.LoginUser;
    public record LoginUserCommand(string Email, string Password, bool RememberMe = false) : IRequest<Result<LoginResponseDto>>;