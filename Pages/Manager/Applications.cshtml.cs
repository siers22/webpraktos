using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using System.Linq;

namespace PRAKTOSWEBAPI.Pages.Manager
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ApplicationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ApplicationViewModel> Applications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var applications = await _context.Applications
                .Include(a => a.Applicant)
                    .ThenInclude(ap => ap.Specialty)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            Applications = applications
                .Where(a => a.Applicant != null && a.Applicant.Specialty != null)
                .Select(a => new ApplicationViewModel
                {
                    Id = a.Id,
                    FullName = a.Applicant!.FullName,
                    SpecialtyName = a.Applicant.Specialty!.Name,
                    AverageScore = a.Applicant.AverageScore,
                    Status = a.Status,
                    SubmittedAt = a.SubmittedAt
                }).ToList();
        }

        public async Task<IActionResult> OnPostAsync(int applicationId, string action)
        {
            var application = await _context.Applications.FindAsync(applicationId);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = action == "approve" ? "Approved" : "Rejected";
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }

    public class ApplicationViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}

