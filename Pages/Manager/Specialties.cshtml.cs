using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Data;
using PRAKTOSWEBAPI.Models;

namespace PRAKTOSWEBAPI.Pages.Manager
{
    [Authorize(Policy = "Manager")]
    public class SpecialtiesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SpecialtiesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Specialty> Specialties { get; set; } = new();

        public async Task OnGetAsync()
        {
            Specialties = await _context.Specialties.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(string action, int? specialtyId, string? name)
        {
            if (action == "add" && !string.IsNullOrEmpty(name))
            {
                var specialty = new Specialty { Name = name };
                _context.Specialties.Add(specialty);
                await _context.SaveChangesAsync();
            }
            else if (action == "edit" && specialtyId.HasValue && !string.IsNullOrEmpty(name))
            {
                var specialty = await _context.Specialties.FindAsync(specialtyId.Value);
                if (specialty != null)
                {
                    specialty.Name = name;
                    await _context.SaveChangesAsync();
                }
            }
            else if (action == "delete" && specialtyId.HasValue)
            {
                var specialty = await _context.Specialties.FindAsync(specialtyId.Value);
                if (specialty != null)
                {
                    _context.Specialties.Remove(specialty);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage();
        }
    }
}


