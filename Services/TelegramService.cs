using Microsoft.Extensions.Configuration;
using PRAKTOSWEBAPI.Services;

namespace PRAKTOSWEBAPI.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly string _botToken;
        private readonly HttpClient _httpClient;

        public TelegramService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _botToken = configuration["Telegram:BotToken"] ?? "5986386293:AAHlwBC_0vqWe-z2Aeo7Dz3jEu5_uPgm4ZI";
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendMessage(long telegramId, string message)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("chat_id", telegramId.ToString()),
                    new KeyValuePair<string, string>("text", message)
                });

                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Telegram API error: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Логируем детальную ошибку для отладки
                Console.WriteLine($"Ошибка отправки Telegram сообщения пользователю {telegramId}: {ex.Message}");
                throw; // Пробрасываем дальше, чтобы контроллер мог обработать
            }
        }
    }
}