using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.CreateSanitationRule;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.PerformAdHocSanitation;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.CancelSanitationTask;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.CompleteSanitationTask;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.GetSanitationDetails;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.StartNewSanitationTask;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.StartSanitationTask;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Tasks.VerifySanitationTask;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sanitation;

public static class SanitationEndpoints
{
    public static void MapSanitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Sanitation")
            .RequireAuthorization();

        group.MapPost(SanitationConstants.RULES, async (CreateSanitationRuleCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(SanitationConstants.WAREHOUSE_DETAILS, async (SanitationTargetType targetType, Guid targetId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSanitationDetailsQuery(targetType, targetId), ct)).ToHttpResult());

        group.MapPost($"{SanitationConstants.TASKS}/start", async (StartSanitationTaskCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost($"{SanitationConstants.TASKS}/complete", async (CompleteSanitationTaskCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(SanitationConstants.TASKS_VERIFY, async (VerifySanitationTaskCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost($"{SanitationConstants.TASKS}/cancel", async (CancelSanitationTaskCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost($"{SanitationConstants.TASKS}/ad-hoc", async (PerformAdHocSanitationCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost($"{SanitationConstants.TASKS}/start-new", async (StartNewSanitationTaskCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());
    }
}