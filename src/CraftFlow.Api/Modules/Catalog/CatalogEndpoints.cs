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
using MediatR;

namespace CraftFlow.Api.Modules.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Catalog")
            .RequireAuthorization();

        // Commands - Create
        group.MapPost(CatalogConstants.UOM, async (CreateUnitOfMeasureCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(CatalogConstants.RAW_MATERIALS, async (CreateRawMaterialCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(CatalogConstants.PRODUCTS, async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(CatalogConstants.RECIPES, async (CreateRecipeCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // Commands - Delete
        group.MapDelete($"{CatalogConstants.UOM}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUnitOfMeasureCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{CatalogConstants.RAW_MATERIALS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRawMaterialCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{CatalogConstants.PRODUCTS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{CatalogConstants.RECIPES}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRecipeCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        // Queries
        group.MapGet(CatalogConstants.UOM, async (ISender sender) =>
        {
            var result = await sender.Send(new GetUnitsOfMeasureQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.RAW_MATERIALS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetRawMaterialsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.PRODUCTS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.RECIPES, async (ISender sender) =>
        {
            var result = await sender.Send(new GetRecipesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}