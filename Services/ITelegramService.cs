namespace PRAKTOSWEBAPI.Services
{
    public interface ITelegramService
    {
        Task SendMessage(long telegramId, string message);
    }
}
