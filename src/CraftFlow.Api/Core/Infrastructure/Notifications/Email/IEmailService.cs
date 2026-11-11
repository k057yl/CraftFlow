namespace CraftFlow.Api.Core.Infrastructure.Notifications.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
    Task<bool> SendOtpCodeAsync(string toEmail, string otpCode, string culture = "uk-UA");
}