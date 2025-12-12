using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using System.Linq;

namespace PRAKTOSWEBAPI.Pages.Admin
{
    [Authorize(Policy = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalUsers { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int TotalSpecialties { get; set; }
        public List<SpecialtyStatViewModel> SpecialtyStats { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            TotalApplications = await _context.Applications.CountAsync();
            ApprovedApplications = await _context.Applications.CountAsync(a => a.Status == "Approved");
            TotalSpecialties = await _context.Specialties.CountAsync();

            var specialties = await _context.Specialties.ToListAsync();
            var applications = await _context.Applications
                .Include(a => a.Applicant)
                    .ThenInclude(ap => ap.Specialty)
                .ToListAsync();

            SpecialtyStats = specialties.Select(s => new SpecialtyStatViewModel
            {
                SpecialtyName = s.Name,
                Total = applications.Count(a => a.Applicant != null && a.Applicant.SpecialtyId == s.Id),
                Approved = applications.Count(a => a.Applicant != null && a.Applicant.SpecialtyId == s.Id && a.Status == "Approved"),
                Pending = applications.Count(a => a.Applicant != null && a.Applicant.SpecialtyId == s.Id && a.Status == "Pending"),
                Rejected = applications.Count(a => a.Applicant != null && a.Applicant.SpecialtyId == s.Id && a.Status == "Rejected")
            }).ToList();
        }
    }

    public class SpecialtyStatViewModel
    {
        public string SpecialtyName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Approved { get; set; }
        public int Pending { get; set; }
        public int Rejected { get; set; }
    }
}

