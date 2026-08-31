using CraftFlow.Api.Modules.Catalog.CreateProduct;
using CraftFlow.Api.Modules.Catalog.CreateRawMaterial;
using CraftFlow.Api.Modules.Catalog.CreateRecipe;
using CraftFlow.Api.Modules.Catalog.CreateUnitOfMeasure;
using CraftFlow.Api.Modules.Catalog.DeleteProduct;
using CraftFlow.Api.Modules.Catalog.DeleteRawMaterial;
using CraftFlow.Api.Modules.Catalog.DeleteRecipe;
using CraftFlow.Api.Modules.Catalog.DeleteUnitOfMeasure;
using CraftFlow.Api.Modules.Catalog.GetProducts;
using CraftFlow.Api.Modules.Catalog.GetRawMaterials;
using CraftFlow.Api.Modules.Catalog.GetRecipes;
using CraftFlow.Api.Modules.Catalog.GetUnitsOfMeasure;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Catalog");

        // Commands - Create
        group.MapPost(Endpoints.UOM, async (CreateUnitOfMeasureCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.RAW_MATERIALS, async (CreateRawMaterialCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.PRODUCTS, async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.RECIPES, async (CreateRecipeCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // Commands - Delete
        group.MapDelete($"{Endpoints.UOM}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUnitOfMeasureCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{Endpoints.RAW_MATERIALS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRawMaterialCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{Endpoints.PRODUCTS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{Endpoints.RECIPES}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRecipeCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        // Queries
        group.MapGet(Endpoints.UOM, async (ISender sender) =>
        {
            var result = await sender.Send(new GetUnitsOfMeasureQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.RAW_MATERIALS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetRawMaterialsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.PRODUCTS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.RECIPES, async (ISender sender) =>
        {
            var result = await sender.Send(new GetRecipesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}