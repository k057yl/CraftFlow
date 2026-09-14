using CraftFlow.Api.Modules.Identity.CreateTenantUser;
using CraftFlow.Api.Modules.Identity.DeleteAccount;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Identity.ExtendSubscription;
using CraftFlow.Api.Modules.Identity.GetOrganizations;
using CraftFlow.Api.Modules.Identity.GetSubscriptionPayments;
using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.Api.Modules.Identity.ManageSubscription;
using CraftFlow.Api.Modules.Identity.RegisterOrganization;
using CraftFlow.Api.Modules.Identity.ResendOtp;
using CraftFlow.Api.Modules.Identity.ToggleOrganizationStatus;
using CraftFlow.Api.Modules.Identity.VerifyOtp;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using System.Security.Claims;

namespace CraftFlow.Api.Modules.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Identity");

        group.MapPost(Endpoints.REGISTER, async (RegisterOrganizationCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("/api/identity/users", async (CreateTenantUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(Endpoints.LOGIN, async (LoginUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.VERIFY_OTP, async (VerifyOtpCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.RESEND_OTP, async (ResendOtpCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost($"{Endpoints.DELETE_ACCOUNT}/confirm", async (DeleteAccountCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet(Endpoints.ORGANIZATIONS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetOrganizationsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(Endpoints.ORGANIZATIONS_EXTED, async (Guid id, ExtendSubscriptionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new ExtendSubscriptionCommand(id, req.Days));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(Endpoints.ORGANIZATIONS_TOGGLE_STATUS, async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ToggleOrganizationStatusCommand(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(Endpoints.ORGANIZATIONS_MANAGE, async (Guid id, ManageSubscriptionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new ManageSubscriptionCommand(id, req.PlanCode, req.AddDays, req.IsActive));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet(Endpoints.ORGANIZATIONS_PAYMENTS, async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetSubscriptionPaymentsQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();
    }
}