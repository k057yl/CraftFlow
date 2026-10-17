using CraftFlow.Api.Modules.Identity.Auth.LoginUser;
using CraftFlow.Api.Modules.Identity.Auth.VerifyOtp;
using CraftFlow.Api.Modules.Identity.Organizations.GetOrganizations;
using CraftFlow.Api.Modules.Identity.Organizations.RegisterOrganization;
using CraftFlow.Api.Modules.Identity.Organizations.ToggleOrganizationStatus;
using CraftFlow.Api.Modules.Identity.Subscriptions.ExtendSubscription;
using CraftFlow.Api.Modules.Identity.Subscriptions.GetSubscriptionPayments;
using CraftFlow.Api.Modules.Identity.Subscriptions.ManageSubscription;
using CraftFlow.Api.Modules.Identity.Users.CreateTenantUser;
using CraftFlow.Api.Modules.Identity.Users.DeleteAccount;
using CraftFlow.Api.Modules.Identity.Users.GetTenantUsers;
using CraftFlow.Api.Modules.Identity.Users.ToggleUserStatus;
using MediatR;

namespace CraftFlow.Api.Modules.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Identity");

        group.MapPost(IdentityConstants.REGISTER, async (RegisterOrganizationCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.USERS, async (CreateTenantUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet(IdentityConstants.USERS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetTenantUsersQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(IdentityConstants.LOGIN, async (LoginUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.VERIFY_OTP, async (VerifyOtpCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.RESEND_OTP, async (Auth.ResendOtp.ResendOtp command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost($"{IdentityConstants.DELETE_ACCOUNT}/confirm", async (DeleteAccountCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet(IdentityConstants.ORGANIZATIONS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetOrganizationsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(IdentityConstants.ORGANIZATIONS_EXTED, async (Guid id, ExtendSubscriptionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new ExtendSubscriptionCommand(id, req.Days));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(IdentityConstants.ORGANIZATIONS_TOGGLE_STATUS, async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ToggleOrganizationStatusCommand(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost(IdentityConstants.ORGANIZATIONS_MANAGE, async (Guid id, ManageSubscriptionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new ManageSubscriptionCommand(id, req.PlanCode, req.AddDays, req.IsActive));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet(IdentityConstants.ORGANIZATIONS_PAYMENTS, async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetSubscriptionPaymentsQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost($"{IdentityConstants.USERS}/{{id:guid}}/toggle-status", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ToggleUserStatusCommand(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();
    }
}