using CraftFlow.Api.Modules.Catalog.Products.CreateProduct;
using CraftFlow.Api.Modules.Catalog.Products.DeleteProduct;
using CraftFlow.Api.Modules.Catalog.Products.GetProducts;
using CraftFlow.Api.Modules.Catalog.RawMaterials.CreateRawMaterial;
using CraftFlow.Api.Modules.Catalog.RawMaterials.DeleteRawMaterial;
using CraftFlow.Api.Modules.Catalog.RawMaterials.GetRawMaterials;
using CraftFlow.Api.Modules.Catalog.Recipes.CreateRecipe;
using CraftFlow.Api.Modules.Catalog.Recipes.DeleteRecipe;
using CraftFlow.Api.Modules.Catalog.Recipes.GetRecipes;
using CraftFlow.Api.Modules.Catalog.UnitsOfMeasure.GetUnitsOfMeasure;
using CraftFlow.Api.Modules.Catalog.UnitsOfMeasure.SeedUnitsOfMeasure;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Catalog")
            .RequireAuthorization();

        // --- COMMANDS (CREATE) ---
        group.MapPost(CatalogConstants.RAW_MATERIALS, async (CreateRawMaterialCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost(CatalogConstants.PRODUCTS, async (CreateProductCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost(CatalogConstants.RECIPES, async (CreateRecipeCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost($"{CatalogConstants.UOM}/seed", async (SeedUnitsOfMeasureCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- COMMANDS (DELETE) ---
        group.MapDelete($"{CatalogConstants.RAW_MATERIALS}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteRawMaterialCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapDelete($"{CatalogConstants.PRODUCTS}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapDelete($"{CatalogConstants.RECIPES}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteRecipeCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- QUERIES ---
        group.MapGet(CatalogConstants.UOM, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUnitsOfMeasureQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.RAW_MATERIALS, async (bool includeArchived, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRawMaterialsQuery(includeArchived), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.PRODUCTS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductsQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(CatalogConstants.RECIPES, async (bool includeArchived, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRecipesQuery(includeArchived), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}