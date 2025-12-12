using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PRAKTOSWEBAPI.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("/logout")]  
        public async Task<IActionResult> Logout()
        {
            
            Response.Cookies.Delete(".AspNetCore.Cookies");

            await HttpContext.SignOutAsync("Cookies");

           
            return Redirect("/");
        }
    }
}