using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using System.Security.Claims;

namespace PRAKTOSWEBAPI.Pages.Student
{
    [Authorize(Policy = "Student")]
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Applicant? Applicant { get; set; }
        public User? CurrentUser { get; set; }
        public Application? Application { get; set; }

        public async Task OnGetAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            CurrentUser = await _context.Users.FindAsync(userId);
            Applicant = await _context.Applicants
                .Include(a => a.Specialty)
                .FirstOrDefaultAsync(a => a.UserId == userId);
            Application = await _context.Applications
                .FirstOrDefaultAsync(a => a.ApplicantId == Applicant!.Id);
        }
    }
}

