using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.Chambers.DeleteChamber;

public record DeleteChamberCommand(Guid Id) : IRequest<Result<bool>>;