using System.Text.RegularExpressions;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Procurement.Domain;

public sealed partial class Supplier : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? LegalName { get; private set; }
    public string? TaxId { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? Notes { get; private set; }

    private Supplier() { }

    public static Supplier Create(
        Guid tenantId,
        string name,
        string? legalName = null,
        string? taxId = null,
        string? phone = null,
        string? email = null,
        string? address = null,
        string? notes = null)
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
            LegalName = SanitizeNullableString(legalName),
            TaxId = SanitizeTaxId(taxId),
            Phone = SanitizePhone(phone),
            Email = SanitizeEmail(email),
            Address = SanitizeNullableString(address),
            Notes = SanitizeNullableString(notes)
        };
    }

    public void UpdateInfo(
        string name,
        string? legalName,
        string? taxId,
        string? phone,
        string? email,
        string? address,
        string? notes)
    {
        var sanitizedName = SanitizeString(name);
        if (string.IsNullOrWhiteSpace(sanitizedName))
            throw new ArgumentException(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED);

        Name = sanitizedName;
        LegalName = SanitizeNullableString(legalName);
        TaxId = SanitizeTaxId(taxId);
        Phone = SanitizePhone(phone);
        Email = SanitizeEmail(email);
        Address = SanitizeNullableString(address);
        Notes = SanitizeNullableString(notes);
    }

    private static string SanitizeString(string input) => input.Trim();
    private static string? SanitizeNullableString(string? input) => string.IsNullOrWhiteSpace(input) ? null : input.Trim();

    private static string? SanitizeTaxId(string? taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId)) return null;
        return DigitsOnlyRegex().Replace(taxId.Trim(), string.Empty);
    }

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