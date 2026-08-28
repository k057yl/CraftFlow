using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.Api.Modules.Identity.RegisterUser;
using MediatR;

namespace CraftFlow.Api.Modules.Identity
{
    public static class IdentityEndpoints
    {
        public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/identity")
                .WithTags("Identity");

            group.MapPost("/register", async (RegisterUserCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPost("/login", async (LoginUserCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });
        }
    }
}
