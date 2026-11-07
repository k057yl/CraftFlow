using Microsoft.Extensions.Options;

namespace CraftFlow.Api.Core.Infrastructure.Security;

public class KeyHasher : IKeyHasher
{
    private readonly ApiKeySecurityOptions _options;

    public KeyHasher(IOptions<ApiKeySecurityOptions> options)
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