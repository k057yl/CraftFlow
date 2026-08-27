using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.SharedKernel.Result;

public record Error(string Code)
{
    public static readonly Error None = new(string.Empty);
    public static readonly Error NullValue = new(ErrorCodes.General.NULL_VALUE);

    public static Error Failure(string code) => new(code);
    public static Error NotFound(string code) => new(code);
    public static Error Validation(string code) => new(code);
    public static Error Conflict(string code) => new(code);
}