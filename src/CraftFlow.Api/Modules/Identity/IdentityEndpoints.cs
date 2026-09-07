using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.Api.Modules.Identity.RegisterUser;
using CraftFlow.Api.Modules.Identity.VerifyOtp;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Identity;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Identity");

        group.MapPost(Endpoints.REGISTER, async (RegisterUserCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

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
    }
}