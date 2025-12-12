using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using System.Linq;

namespace PRAKTOSWEBAPI.Pages.Admin
{
    [Authorize(Policy = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UsersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<User> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            Users = await _context.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int userId, string action, string? newRole)
        {
            if (action == "toggleRole" && !string.IsNullOrEmpty(newRole))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.Role = newRole;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage();
        }
    }
}

