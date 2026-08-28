using CraftFlow.Api.Modules.Catalog.CreateProduct;
using CraftFlow.Api.Modules.Catalog.CreateRawMaterial;
using CraftFlow.Api.Modules.Catalog.CreateRecipe;
using CraftFlow.Api.Modules.Catalog.CreateUnitOfMeasure;
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
        var group = app.MapGroup("api/catalog")
            .WithTags("Catalog");

        // Commands
        group.MapPost("/units-of-measure", async (CreateUnitOfMeasureCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/raw-materials", async (CreateRawMaterialCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/products", async (CreateProductCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/recipes", async (CreateRecipeCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // Queries
        group.MapGet("/units-of-measure", async (ISender sender) =>
        {
            var result = await sender.Send(new GetUnitsOfMeasureQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/raw-materials", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRawMaterialsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/products", async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/recipes", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRecipesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}