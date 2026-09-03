using System.Text;
using System.Text.Json;

namespace CraftFlow.Api.Common.BackgroundWorkers;

public class TelegramNotificationService : ITelegramNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramNotificationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAdminNotificationAsync(string message, CancellationToken cancellationToken = default)
    {
        var botToken = _configuration["Telegram:BotToken"];
        var chatId = _configuration["Telegram:AdminChatId"];

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
        {
            _logger.LogWarning("Telegram BotToken или AdminChatId не настроены в appsettings.json.");
            return;
        }

        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

        var payload = new
        {
            chat_id = chatId,
            text = message,
            parse_mode = "Markdown"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Ошибка отправки сообщения в Telegram: {Error}", errorResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Сбой при отправке сообщения в Telegram.");
        }
    }
}
