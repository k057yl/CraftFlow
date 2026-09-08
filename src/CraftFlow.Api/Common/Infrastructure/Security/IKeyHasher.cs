namespace CraftFlow.Api.Common.Infrastructure.Security;
public interface IKeyHasher
{
    string ComputeHash(string rawKey);
}