using CraftFlow.Api.Modules.Catalog.GetRecipes;
using CraftFlow.Api.Modules.Production.CompleteProductionBatch;
using CraftFlow.Api.Modules.Production.GetActiveBatches;
using CraftFlow.Api.Modules.Production.GetBatchCost;
using CraftFlow.Api.Modules.Production.StartProductionBatch;
using MediatR;

namespace CraftFlow.Api.Modules.Production
{
    public static class ProductionEndpoints
    {
        public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/production")
                .WithTags("Production");

            group.MapPost("/batches/start", async (StartProductionBatchCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPost("/batches/complete", async (CompleteProductionBatchCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/batches/active", async (ISender sender) =>
            {
                var result = await sender.Send(new GetActiveBatchesQuery());
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/costing/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetBatchCostQuery(id));
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });
        }
    }
}
