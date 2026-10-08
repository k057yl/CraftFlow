using System.Text.RegularExpressions;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Procurement.Domain;

public sealed partial class Supplier : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    private Supplier() { }

    public static Supplier Create(Guid tenantId, string name, string? phone = null, string? email = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        var sanitizedName = SanitizeString(name);
        if (string.IsNullOrWhiteSpace(sanitizedName))
            throw new ArgumentException(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED);

        return new Supplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = sanitizedName,
            Phone = SanitizePhone(phone),
            Email = SanitizeEmail(email)
        };
    }

    public void UpdateInfo(string name, string? phone, string? email)
    {
        var sanitizedName = SanitizeString(name);
        if (string.IsNullOrWhiteSpace(sanitizedName))
            throw new ArgumentException(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED);

        Name = sanitizedName;
        Phone = SanitizePhone(phone);
        Email = SanitizeEmail(email);
    }

    private static string SanitizeString(string input) => input.Trim();

    private static string? SanitizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        return email.Trim().ToLowerInvariant();
    }

    private static string? SanitizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;

        var trimmed = phone.Trim();
        bool hasPlus = trimmed.StartsWith('+');
        var digitsOnly = DigitsOnlyRegex().Replace(trimmed, string.Empty);

        if (string.IsNullOrEmpty(digitsOnly)) return null;

        return hasPlus ? $"+{digitsOnly}" : digitsOnly;
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex DigitsOnlyRegex();
}