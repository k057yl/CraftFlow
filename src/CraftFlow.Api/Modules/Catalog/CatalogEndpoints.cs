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
        var group = app.MapGroup("api/catalog")
            .WithTags("Catalog");

        // Commands - Create
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

        // Commands - Delete
        group.MapDelete("/units-of-measure/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUnitOfMeasureCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete("/raw-materials/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRawMaterialCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete("/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapDelete("/recipes/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteRecipeCommand(id));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
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