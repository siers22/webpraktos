using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PRAKTOSWEBAPI.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("/logout")]  // ← по этому URL будет выход
        public async Task<IActionResult> Logout()
        {
            // Удаляем куку по имени — работает всегда
            Response.Cookies.Delete(".AspNetCore.Cookies");

            // И на всякий случай SignOut
            await HttpContext.SignOutAsync("Cookies");

            // Редирект на главную — тупо и надёжно
            return Redirect("/");
        }
    }
}