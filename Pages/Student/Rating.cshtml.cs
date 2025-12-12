using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;
using System.Linq;

namespace PRAKTOSWEBAPI.Pages.Student
{
    [Authorize(Policy = "Student")]
    public class RatingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RatingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<RatingViewModel> Ratings { get; set; } = new();
        public List<Specialty> Specialties { get; set; } = new();

        public async Task OnGetAsync()
        {
            Specialties = await _context.Specialties.ToListAsync();

            var applicants = await _context.Applicants
                .Include(a => a.User)
                .Include(a => a.Specialty)
                .Where(a => a.User != null && a.User.IsConfirmed && a.Specialty != null)
                .ToListAsync();
            
            var applications = await _context.Applications.ToListAsync();

            var ratings = applicants
                .GroupBy(a => a.SpecialtyId)
                .SelectMany(g => g
                    .OrderByDescending(a => a.AverageScore)
                    .Select((a, index) => new RatingViewModel
                    {
                        FullName = a.FullName,
                        SpecialtyName = a.Specialty?.Name ?? "Не указано",
                        SpecialtyId = a.SpecialtyId,
                        AverageScore = a.AverageScore,
                        Status = applications.FirstOrDefault(app => app.ApplicantId == a.Id)?.Status ?? "Pending",
                        Rank = index + 1
                    }))
                .OrderByDescending(r => r.AverageScore)
                .ToList();

            // Recalculate ranks globally
            var globalRank = 1;
            foreach (var rating in ratings.OrderByDescending(r => r.AverageScore))
            {
                rating.Rank = globalRank++;
            }

            Ratings = ratings;
        }
    }

    public class RatingViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int SpecialtyId { get; set; }
        public decimal AverageScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Rank { get; set; }
    }
}

