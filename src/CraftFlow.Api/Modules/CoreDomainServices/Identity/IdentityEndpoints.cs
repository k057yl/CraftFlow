using CraftFlow.Api.Modules.CoreDomainServices.Identity.Auth.LoginUser;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Auth.VerifyOtp;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Organizations.GetOrganizations;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Organizations.RegisterOrganization;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Organizations.ToggleOrganizationStatus;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.CreateTenantUser;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.DeleteAccount;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.GetTenantUsers;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.GetUsersLookup;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.ToggleUserStatus;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ExtendSubscription;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetSubscriptionPayments;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ManageSubscription;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Identity");

        // --- PUBLIC ENDPOINTS (No Auth) ---
        group.MapPost(IdentityConstants.REGISTER, async (RegisterOrganizationCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.LOGIN, async (LoginUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.VERIFY_OTP, async (VerifyOtpCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(IdentityConstants.RESEND_OTP, async (Auth.ResendOtp.ResendOtp command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // --- AUTHORIZED ENDPOINTS ---
        var authGroup = group.MapGroup(string.Empty).RequireAuthorization();

        // Account Self-Management
        authGroup.MapPost($"{IdentityConstants.DELETE_ACCOUNT}/confirm", async (DeleteAccountCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // Tenant Users Management (USERS_MANAGE)
        authGroup.MapPost(IdentityConstants.USERS, async (CreateTenantUserCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapGet(IdentityConstants.USERS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTenantUsersQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapPost($"{IdentityConstants.USERS}/{{id:guid}}/toggle-status", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ToggleUserStatusCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // Platform Administration
        authGroup.MapGet(IdentityConstants.ORGANIZATIONS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetOrganizationsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_EXTED, async (Guid id, ExtendSubscriptionRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ExtendSubscriptionCommand(id, req.Days), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_TOGGLE_STATUS, async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ToggleOrganizationStatusCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_MANAGE, async (Guid id, ManageSubscriptionRequest req, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ManageSubscriptionCommand(id, req.PlanCode, req.AddDays, req.IsActive), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapGet(IdentityConstants.ORGANIZATIONS_PAYMENTS, async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSubscriptionPaymentsQuery(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        authGroup.MapGet($"{IdentityConstants.USERS}/lookup", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetUsersLookupQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        });
    }
}