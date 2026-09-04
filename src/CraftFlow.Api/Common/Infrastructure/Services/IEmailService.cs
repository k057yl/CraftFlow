namespace CraftFlow.Api.Infrastructure.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
    Task<bool> SendOtpCodeAsync(string toEmail, string otpCode, string culture = "uk-UA");
}