namespace CraftFlow.Api.Core.MultiTenancy;

public record TenantKeyValidationResult
{
    public bool IsValid { get; init; }
    public Guid TenantId { get; init; }
    public Guid AccessKeyId { get; init; }
    public string? ErrorCode { get; init; }

    public static TenantKeyValidationResult Success(Guid tenantId, Guid accessKeyId) =>
        new() { IsValid = true, TenantId = tenantId, AccessKeyId = accessKeyId };

    public static TenantKeyValidationResult Failure(string errorCode) =>
        new() { IsValid = false, ErrorCode = errorCode };
}