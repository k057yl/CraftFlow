using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.LoginUser
{
    public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginResponseDto>>;
}
