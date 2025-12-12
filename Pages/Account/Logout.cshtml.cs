using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRAKTOSWEBAPI.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // Удаляем куку принудительно — по имени
            Response.Cookies.Delete(".AspNetCore.Cookies", new CookieOptions
            {
                Path = "/",
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

            Response.Cookies.Delete(".AspNetCore.Cookies"); // и без опций

            // Принудительно выходим
            await HttpContext.SignOutAsync("Cookies");

           
            return Redirect("~/"); 
           
        }
    }
}