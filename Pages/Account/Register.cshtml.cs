using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace PRAKTOSWEBAPI.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegisterModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public List<Specialty> Specialties { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Specialties = await _context.Specialties.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var registerData = new
                {
                    TelegramId = Input.TelegramId,
                    Username = Input.Username,
                    Password = Input.Password,
                    FullName = Input.FullName,
                    PassportNumber = Input.PassportNumber,
                    EducationLevel = Input.EducationLevel,
                    Grade1 = Input.Grade1,
                    Grade2 = Input.Grade2,
                    Grade3 = Input.Grade3,
                    Grade4 = Input.Grade4,
                    Grade5 = Input.Grade5,
                    SpecialtyId = Input.SpecialtyId
                };

                var json = JsonSerializer.Serialize(registerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{baseUrl}/api/auth/register-applicant", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Заявка успешно подана! Теперь подтвердите регистрацию через Telegram бота.";
                    Specialties = await _context.Specialties.ToListAsync();
                    return Page();
                }
                else
                {
                    ErrorMessage = "Ошибка при регистрации. " + responseContent;
                    Specialties = await _context.Specialties.ToListAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Произошла ошибка: " + ex.Message;
                Specialties = await _context.Specialties.ToListAsync();
                return Page();
            }
        }
    }

    public class RegisterInputModel
    {
        [Required(ErrorMessage = "Telegram ID обязателен")]
        public long TelegramId { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "ФИО обязательно")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Номер паспорта обязателен")]
        public string PassportNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Уровень образования обязателен")]
        public string EducationLevel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Специальность обязательна")]
        public int SpecialtyId { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade1 { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade2 { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade3 { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade4 { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade5 { get; set; }
    }
}

