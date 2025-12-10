using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRAKTOSWEBAPI.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Applicant")]
        public int ApplicantId { get; set; }
        public Applicant Applicant { get; set; }  // Many-to-one

        [Required]
        public string FilePath { get; set; }  // Путь к файлу на сервере, e.g. "/uploads/passport.jpg"
        
        [Required]
        public string Type { get; set; }  // e.g. "Passport", "Attestat"
    }
}