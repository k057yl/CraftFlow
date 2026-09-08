namespace CraftFlow.Api.Modules.Subscriptions.GenerateAccessKey;
public record GenerateAccessKeyResponse(string RawKey, string KeyName, string KeyPrefix, string KeySuffix);