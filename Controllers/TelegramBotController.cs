using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using PRAKTOSWEBAPI.Services;

namespace PRAKTOSWEBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TelegramBotController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITelegramService _telegramService;
        private readonly IConfiguration _configuration;

        public TelegramBotController(ApplicationDbContext context, ITelegramService telegramService, IConfiguration configuration)
        {
            _context = context;
            _telegramService = telegramService;
            _configuration = configuration;
        }

        [HttpPost("webhook")]
        [AllowAnonymous] // Webhook должен быть доступен без авторизации
        public async Task<IActionResult> Webhook([FromBody] TelegramUpdate update)
        {
            if (update?.Message == null)
                return Ok();

            var chatId = update.Message?.Chat?.Id ?? 0;
            var messageText = update.Message?.Text?.Trim() ?? "";
            var userId = update.Message?.From?.Id ?? 0;

            // Игнорируем сообщения от ботов
            if (update.Message.From?.IsBot == true)
                return Ok();

            // Обработка команды /start
            if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                await _telegramService.SendMessage(chatId, 
                    "👋 Добро пожаловать!\n\n" +
                    "Этот бот используется для подтверждения регистрации в системе поступления.\n\n" +
                    "Если вы подали заявку на сайте, просто напишите любое сообщение боту для подтверждения регистрации.");
                return Ok();
            }

            try
            {
                // Проверяем, есть ли неподтвержденная регистрация для этого TelegramId
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.TelegramId == userId);

                if (user == null)
                {
                    // Пользователь не найден - отправляем сообщение
                    await _telegramService.SendMessage(chatId, 
                        "❌ Вы не зарегистрированы в системе.\n\n" +
                        "Пожалуйста, сначала подайте заявку на сайте.");
                    return Ok();
                }

                // Если пользователь уже подтвержден
                if (user.IsConfirmed)
                {
                    var applicant = await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == user.Id);
                    await _telegramService.SendMessage(chatId, 
                        "✅ Ваша регистрация уже подтверждена!\n\n" +
                        $"Ваш логин: {user.Username}\n" +
                        $"ФИО: {applicant?.FullName ?? "Не указано"}\n\n" +
                        "Используйте ваш логин и пароль для входа на сайте.");
                    return Ok();
                }

                // Подтверждаем регистрацию
                user.IsConfirmed = true;
                await _context.SaveChangesAsync();

                // Отправляем данные для входа
                var applicantInfo = await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == user.Id);

                await _telegramService.SendMessage(chatId, 
                    "✅ Регистрация подтверждена!\n\n" +
                    $"Ваш логин: {user.Username}\n" +
                    $"ФИО: {applicantInfo?.FullName ?? "Не указано"}\n\n" +
                    "Теперь вы можете войти в систему, используя ваш логин и пароль, которые вы указали при регистрации на сайте.");

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки сообщения от Telegram: {ex.Message}");
                return Ok(); // Всегда возвращаем 200, чтобы Telegram не повторял запрос
            }
        }

        [HttpPost("setup-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> SetupWebhook([FromQuery] string? url = null)
        {
            try
            {
                var botToken = _configuration["Telegram:BotToken"];
                if (string.IsNullOrEmpty(botToken))
                {
                    return BadRequest("BotToken не настроен в appsettings.json");
                }

                // Если URL не указан, используем текущий домен
                if (string.IsNullOrEmpty(url))
                {
                    var scheme = Request.Scheme;
                    var host = Request.Host;
                    url = $"{scheme}://{host}/api/telegrambot/webhook";
                }

                var webhookUrl = $"https://api.telegram.org/bot{botToken}/setWebhook?url={Uri.EscapeDataString(url)}";
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(webhookUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { 
                        message = "Webhook успешно настроен", 
                        url = url,
                        response = content 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        message = "Ошибка настройки webhook", 
                        response = content 
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Модели для десериализации Telegram Update
    public class TelegramUpdate
    {
        public TelegramMessage? Message { get; set; }
    }

    public class TelegramMessage
    {
        public long MessageId { get; set; }
        public TelegramUser? From { get; set; }
        public TelegramChat? Chat { get; set; }
        public string? Text { get; set; }
    }

    public class TelegramUser
    {
        public long Id { get; set; }
        public bool IsBot { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
    }

    public class TelegramChat
    {
        public long Id { get; set; }
        public string? Type { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
    }
}
