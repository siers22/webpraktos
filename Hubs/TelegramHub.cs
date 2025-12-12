using Microsoft.AspNetCore.SignalR;

namespace PRAKTOSWEBAPI.Hubs
{
    public class TelegramHub : Hub
    {
        public async Task ConfirmRegistration(long telegramId)
        {
            // Это вызовется с бота — и сразу подтвердит регистрацию
            await Task.CompletedTask; // будет логика в клиенте
        }
    }
}