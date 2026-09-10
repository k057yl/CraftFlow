using System.Security.Claims;
using CraftFlow.Api.Modules.Identity.CreateTenantUser;
using CraftFlow.Api.Modules.Identity.DeleteAccount;
using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.Api.Modules.Identity.RegisterOrganization;
using CraftFlow.Api.Modules.Identity.ResendOtp;
using CraftFlow.Api.Modules.Identity.VerifyOtp;
using CraftFlow.SharedKernel.Constants;
using MediatR;

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

        group.MapDelete(Endpoints.DELETE_ACCOUNT, async (ClaimsPrincipal user, ISender sender) =>
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await sender.Send(new DeleteAccountCommand(userId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();
    }
}