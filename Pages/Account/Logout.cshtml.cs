using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRAKTOSWEBAPI.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            return await PerformLogout();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return await PerformLogout();
        }

        private async Task<IActionResult> PerformLogout()
        {
            // Очищаем все cookies аутентификации с правильными параметрами
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(-1), // Устанавливаем в прошлое
                AllowRefresh = false
            };

            // Выходим из схемы аутентификации - это удалит cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authProperties);
            
            // Дополнительно удаляем cookie вручную с правильными параметрами
            // ASP.NET Core использует имя схемы как имя cookie, но может быть и другое
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };
            
            // Удаляем cookie с разными возможными именами
            // По умолчанию ASP.NET Core использует имя схемы "Cookies"
            var possibleCookieNames = new[]
            {
                "Cookies", // Имя схемы
                ".AspNetCore.Cookies", // Стандартное имя
                CookieAuthenticationDefaults.CookiePrefix + CookieAuthenticationDefaults.AuthenticationScheme
            };
            
            foreach (var name in possibleCookieNames)
            {
                // Удаляем cookie
                Response.Cookies.Delete(name, cookieOptions);
                // Также устанавливаем пустое значение с истекшим сроком для гарантии
                Response.Cookies.Append(name, "", cookieOptions);
            }
            
            // Очищаем сессию
            if (HttpContext.Session != null)
            {
                HttpContext.Session.Clear();
            }

            // Редирект на главную страницу
            // Используем Redirect с полным путем для гарантированного сброса
            return Redirect("/Index");
        }
    }
}

