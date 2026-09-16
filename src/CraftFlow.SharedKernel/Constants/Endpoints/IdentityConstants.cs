namespace CraftFlow.Api.Modules.Identity;
public class IdentityConstants
{
    public const string REGISTER = "api/identity/register";
    public const string LOGIN = "api/identity/login";
    public const string USERS = "/api/identity/users";
    public const string VERIFY_OTP = "api/identity/verify-otp";
    public const string RESEND_OTP = "api/identity/resend-otp";
    public const string DELETE_ACCOUNT = "api/identity/account";
    public const string ORGANIZATIONS = "/api/identity/organizations";
    public const string ORGANIZATIONS_EXTED = "/api/identity/organizations/{id:guid}/extend";
    public const string ORGANIZATIONS_TOGGLE_STATUS = "/api/identity/organizations/{id:guid}/toggle-status";
    public const string ORGANIZATIONS_MANAGE = "/api/identity/organizations/{id:guid}/manage";
    public const string ORGANIZATIONS_PAYMENTS = "/api/identity/organizations/{id:guid}/payments";
}