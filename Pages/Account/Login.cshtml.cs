using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BCrypt.Net;

namespace PRAKTOSWEBAPI.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

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
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Username == Input.Username || u.TelegramId.ToString() == Input.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
                {
                    ErrorMessage = "Неверный логин или пароль";
                    return Page();
                }

                if (!user.IsConfirmed)
                {
                    ErrorMessage = "Регистрация не подтверждена. Пожалуйста, подтвердите регистрацию через Telegram бота.";
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirect based on role
                return user.Role switch
                {
                    "Admin" => RedirectToPage("/Admin/Dashboard"),
                    "Manager" => RedirectToPage("/Manager/Applications"),
                    "Applicant" => RedirectToPage("/Student/Rating"),
                    _ => RedirectToPage("/Index")
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = "Произошла ошибка: " + ex.Message;
                return Page();
            }
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "Логин обязателен")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}

