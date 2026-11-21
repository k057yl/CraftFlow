using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.MRP.CalculateRequirements;
using CraftFlow.Api.Modules.CoreDomainServices.MRP.CreateProcurement;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.MRP;

public static class MrpEndpoints
{
    public static void MapMrpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("MRP")
            .RequireAuthorization();

        group.MapPost(MrpConstants.MRP_REQUIREMENTS, async (CalculateRequirementsQuery query, ISender sender, CancellationToken ct) =>
            (await sender.Send(query, ct)).ToHttpResult());

        group.MapPost(MrpConstants.MRP_CREATE_PURCHARE_ORDER, async (CreateProcurementFromMrpCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());
    }
}