using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using PRAKTOSWEBAPI.Services;
using System.Text;

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

                // Генерируем пароль, если его ещё нет (на случай, если он уже был, но не отправлен)
                if (string.IsNullOrEmpty(user.TempPassword))
                {
                    user.TempPassword = GenerateSecurePassword();
                    // Хешируем и сохраняем как основной пароль
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.TempPassword);
                }

                await _context.SaveChangesAsync();

                // Получаем инфу абитуриента
                var applicantInfo = await _context.Applicants.FirstOrDefaultAsync(a => a.UserId == user.Id);

                // Отправляем логин + пароль в личку
                await _telegramService.SendMessage(chatId,
                    $"Регистрация подтверждена!\n\n" +
                    $"Ваш логин: `{user.Username}`\n" +
                    $"Ваш временный пароль: `{user.TempPassword}`\n\n" +
                    $"ФИО: {applicantInfo?.FullName ?? "Не указано"}\n\n"
                    );

                
                user.TempPassword = null;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки сообщения от Telegram: {ex.Message}");
                return Ok(); 
            }
        }
        private string GenerateSecurePassword(int length = 10)
        {
            const string lower = "abcdefghjkmnpqrstuvwxyz";
            const string upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
            const string digits = "23456789";
            const string special = "!@#$%";
            var all = lower + upper + digits + special;

            var rnd = Random.Shared;
            var password = new StringBuilder();

            // Гарантируем по одному символу из каждой группы
            password.Append(lower[rnd.Next(lower.Length)]);
            password.Append(upper[rnd.Next(upper.Length)]);
            password.Append(digits[rnd.Next(digits.Length)]);
            password.Append(special[rnd.Next(special.Length)]);

            // Дополняем до нужной длины
            while (password.Length < length)
                password.Append(all[rnd.Next(all.Length)]);

            // Перемешиваем
            var chars = password.ToString().ToCharArray();
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = rnd.Next(0, i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
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