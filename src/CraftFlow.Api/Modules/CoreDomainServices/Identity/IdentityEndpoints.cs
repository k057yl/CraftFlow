using CraftFlow.Api.Core.Extensions;
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
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(IdentityConstants.LOGIN, async (LoginUserCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(IdentityConstants.VERIFY_OTP, async (VerifyOtpCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(IdentityConstants.RESEND_OTP, async (Auth.ResendOtp.ResendOtp command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        // --- AUTHORIZED ENDPOINTS ---
        var authGroup = group.MapGroup(string.Empty).RequireAuthorization();

        // Account Self-Management
        authGroup.MapPost($"{IdentityConstants.DELETE_ACCOUNT}/confirm", async (DeleteAccountCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        // Tenant Users Management (USERS_MANAGE)
        authGroup.MapPost(IdentityConstants.USERS, async (CreateTenantUserCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        authGroup.MapGet(IdentityConstants.USERS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetTenantUsersQuery(), ct)).ToHttpResult());

        authGroup.MapPost($"{IdentityConstants.USERS}/{{id:guid}}/toggle-status", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ToggleUserStatusCommand(id), ct)).ToHttpResult());

        // Platform Administration
        authGroup.MapGet(IdentityConstants.ORGANIZATIONS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetOrganizationsQuery(), ct)).ToHttpResult());

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_EXTED, async (Guid id, ExtendSubscriptionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ExtendSubscriptionCommand(id, req.Days), ct)).ToHttpResult());

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_TOGGLE_STATUS, async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ToggleOrganizationStatusCommand(id), ct)).ToHttpResult());

        authGroup.MapPost(IdentityConstants.ORGANIZATIONS_MANAGE, async (Guid id, ManageSubscriptionRequest req, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ManageSubscriptionCommand(id, req.PlanCode, req.AddDays, req.IsActive), ct)).ToHttpResult());

        authGroup.MapGet(IdentityConstants.ORGANIZATIONS_PAYMENTS, async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSubscriptionPaymentsQuery(id), ct)).ToHttpResult());

        authGroup.MapGet($"{IdentityConstants.USERS}/lookup", async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUsersLookupQuery(), ct)).ToHttpResult());
    }
}