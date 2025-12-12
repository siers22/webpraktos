using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;

namespace PRAKTOSWEBAPI.Pages.Admin
{
    [Authorize(Policy = "Admin")]
    public class CreateUserModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateUserModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Проверяем уникальность TelegramId
                if (await _context.Users.AnyAsync(u => u.TelegramId == Input.TelegramId))
                {
                    ErrorMessage = "Пользователь с таким Telegram ID уже существует";
                    return Page();
                }

                // Проверяем уникальность Username
                if (await _context.Users.AnyAsync(u => u.Username == Input.Username))
                {
                    ErrorMessage = "Пользователь с таким логином уже существует";
                    return Page();
                }

                // Создаем пользователя (админы и менеджеры не требуют подтверждения)
                var user = new User
                {
                    TelegramId = Input.TelegramId,
                    Username = Input.Username,
                    Role = Input.Role,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password),
                    IsConfirmed = true // Админы и менеджеры сразу подтверждены
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                SuccessMessage = $"Пользователь '{Input.Username}' успешно создан с ролью '{Input.Role}'";
                Input = new CreateInputModel(); // Очищаем форму
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Произошла ошибка: " + ex.Message;
                return Page();
            }
        }
    }

    public class CreateInputModel
    {
        [Required(ErrorMessage = "Telegram ID обязателен")]
        public long TelegramId { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Роль обязательна")]
        public string Role { get; set; } = string.Empty;
    }
}

