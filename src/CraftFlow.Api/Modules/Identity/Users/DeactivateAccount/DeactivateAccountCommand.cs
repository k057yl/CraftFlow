using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Users.DeactivateAccount;

public record DeactivateAccountCommand(string ConfirmationEmail) : IRequest<Result<bool>>;