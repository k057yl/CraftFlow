using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;

namespace CraftFlow.Api.Core.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result, bool notFoundOnError = false) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : MapError(result.Error, notFoundOnError);

    public static IResult ToHttpResult(this Result result, bool noContent = false) =>
        result.IsSuccess
            ? (noContent ? Results.NoContent() : Results.Ok())
            : MapError(result.Error, false);

    private static IResult MapError(Error error, bool notFoundOnError)
    {
        if (error.Code == ErrorCodes.Auth.ACCESS_DENIED)
            return Results.Json(new { Message = error.Code }, statusCode: StatusCodes.Status403Forbidden);

        if (notFoundOnError || error.Code == ErrorCodes.Inventory.ITEM_NOT_FOUND)
            return Results.NotFound(error);

        return Results.BadRequest(error);
    }
}