namespace CraftFlow.Api.Common.Infrastructure.Security;
public class KeyHasher : IKeyHasher
{
    private readonly ApiKeySecurityOptions _options;

    public KeyHasher(Microsoft.Extensions.Options.IOptions<ApiKeySecurityOptions> options)
    {
        _options = options.Value;
    }

    public string ComputeHash(string rawKey)
    {
        var payload = $"{rawKey}:{_options.ServerPepper}";
        var bytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var hashBytes = System.Security.Cryptography.SHA256.HashData(bytes);

        return Convert.ToHexString(hashBytes);
    }
}