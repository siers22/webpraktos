using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using PRAKTOSWEBAPI.Services;
using BCrypt.Net;

namespace PRAKTOSWEBAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ITelegramService _telegramService;

        public AuthController(ApplicationDbContext context, IConfiguration config, ITelegramService telegramService)
        {
            _context = context;
            _config = config;
            _telegramService = telegramService;
        }

        [HttpPost("register-applicant")]
        public async Task<IActionResult> RegisterApplicant([FromBody] RegisterModel model)
        {
            // Валидация (TelegramId уникальный?)
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.TelegramId == model.TelegramId);
            if (existingUser != null)
            {
                return BadRequest(new { 
                    error = "TelegramId already exists", 
                    message = $"Пользователь с Telegram ID {model.TelegramId} уже зарегистрирован.",
                    username = existingUser.Username,
                    isConfirmed = existingUser.IsConfirmed
                });
            }

            // Валидация Username (если не указан, генерируем автоматически)
            var username = string.IsNullOrWhiteSpace(model.Username) 
                ? $"applicant_{model.TelegramId}" 
                : model.Username;

            // Проверяем уникальность Username
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return BadRequest(new { 
                    error = "Username already exists", 
                    message = $"Логин '{username}' уже занят. Выберите другой."
                });
            }

            // Создаем неподтвержденного пользователя
            var user = new User
            {
                TelegramId = model.TelegramId,
                Username = username,
                Role = "Applicant",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsConfirmed = false // Не подтвержден до обращения к боту
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var applicant = new Applicant
            {
                UserId = user.Id,
                FullName = model.FullName,
                PassportNumber = model.PassportNumber,
                EducationLevel = model.EducationLevel,
                Grade1 = model.Grade1,
                Grade2 = model.Grade2,
                Grade3 = model.Grade3,
                Grade4 = model.Grade4,
                Grade5 = model.Grade5,
                SpecialtyId = model.SpecialtyId
            };
            applicant.CalculateAverageScore();

            _context.Applicants.Add(applicant);
            await _context.SaveChangesAsync();

            
            var app = new Application
            {
                Id = applicant.Id,
                ApplicantId = applicant.Id,
                Status = "Pending"
            };
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Заявка на регистрацию создана. Подтвердите регистрацию, написав боту в Telegram.",
                username = user.Username,
                telegramId = model.TelegramId,
                isConfirmed = false,
                note = "Напишите боту в Telegram с вашего аккаунта для подтверждения регистрации"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Можно войти по Username или по TelegramId
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Username == model.Username || u.TelegramId.ToString() == model.Username);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized();

            // Проверяем, подтверждена ли регистрация
            if (!user.IsConfirmed)
            {
                return BadRequest(new { 
                    error = "Registration not confirmed",
                    message = "Регистрация не подтверждена. Напишите боту в Telegram для подтверждения."
                });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, user.Role) };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddHours(1), signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterModel
    {
        public long TelegramId { get; set; }
        public string Username { get; set; } // Логин, который указывает студент
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string EducationLevel { get; set; }
        public int Grade1 { get; set; }
        public int Grade2 { get; set; }
        public int Grade3 { get; set; }
        public int Grade4 { get; set; }
        public int Grade5 { get; set; }
        public int SpecialtyId { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; } 
        public string Password { get; set; }
    }
}