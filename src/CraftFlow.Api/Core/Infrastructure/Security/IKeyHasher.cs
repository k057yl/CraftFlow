namespace CraftFlow.Api.Core.Infrastructure.Security;
public interface IKeyHasher
{
    string ComputeHash(string rawKey);
}