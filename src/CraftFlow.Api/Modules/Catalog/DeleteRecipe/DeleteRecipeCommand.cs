using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.DeleteRecipe;

public record DeleteRecipeCommand(Guid Id) : IRequest<Result>;