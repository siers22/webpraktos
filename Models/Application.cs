using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRAKTOSWEBAPI.Models
{
    public class Application
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Applicant")]
        public int ApplicantId { get; set; }
        public Applicant Applicant { get; set; }

        public string Status { get; set; } = "Pending";  // "Pending", "Approved", "Rejected"

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}