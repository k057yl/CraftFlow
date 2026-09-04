using CraftFlow.Api.Common.Infrastructure.Services;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace CraftFlow.Api.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(HttpClient httpClient, IConfiguration config, ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var apiKey = config["Brevo:ApiKey"] ?? throw new ArgumentNullException(nameof(config), "BREVO_API_KEY_MISSING");
        _fromEmail = config["Brevo:FromEmail"] ?? throw new ArgumentNullException(nameof(config), "BREVO_FROM_EMAIL_MISSING");
        _fromName = config["Brevo:FromName"] ?? "CraftFlow System";

        _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
    {
        var payload = new BrevoSendEmailRequest
        {
            Sender = new BrevoEmailEntity { Name = _fromName, Email = _fromEmail },
            To = [new BrevoEmailEntity { Email = toEmail }],
            Subject = subject,
            HtmlContent = htmlContent
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("smtp/email", payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("EMAIL_SENT_SUCCESSFULLY: {Email}", toEmail);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("BREVO_API_ERROR: Status {Status}, Details: {Error}", response.StatusCode, errorContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FAILED_TO_SEND_EMAIL: {Email}", toEmail);
            return false;
        }
    }

    public async Task<bool> SendOtpCodeAsync(string toEmail, string otpCode, string culture = "uk-UA")
    {
        var (subject, htmlContent) = EmailTemplates.GetOtpTemplate(otpCode, culture);
        return await SendEmailAsync(toEmail, subject, htmlContent);
    }

    private sealed class BrevoSendEmailRequest
    {
        [JsonPropertyName("sender")]
        public BrevoEmailEntity Sender { get; init; } = null!;

        [JsonPropertyName("to")]
        public List<BrevoEmailEntity> To { get; init; } = [];

        [JsonPropertyName("subject")]
        public string Subject { get; init; } = null!;

        [JsonPropertyName("htmlContent")]
        public string HtmlContent { get; init; } = null!;
    }

    private sealed class BrevoEmailEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; } = null!;
    }
}