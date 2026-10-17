using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Users.DeleteAccount;

public record DeleteAccountCommand(string ConfirmationEmail) : IRequest<Result<bool>>;