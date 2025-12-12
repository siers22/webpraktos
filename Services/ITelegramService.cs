using System;

namespace PRAKTOSWEBAPI.Services
{
    public interface ITelegramService
    {
        Task SendMessage(long telegramId, string message);
        event Func<long, Task> RegistrationConfirmed; 
    }
}