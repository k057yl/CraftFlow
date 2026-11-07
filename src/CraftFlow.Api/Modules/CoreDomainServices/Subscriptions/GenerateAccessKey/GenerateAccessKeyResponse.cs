namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GenerateAccessKey;
public record GenerateAccessKeyResponse(string RawKey, string KeyName, string KeyPrefix, string KeySuffix);