using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRAKTOSWEBAPI.Models
{
    public class Applicant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }  // FK to User.Id

        [Required]
        public string FullName { get; set; }  // ФИО

        [Required]
        public string PassportNumber { get; set; }  // Номер паспорта

        [Required]
        public string EducationLevel { get; set; } // Имеющееся образование
        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade1 { get; set; }
        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade2 { get; set; }

        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade3 { get; set; }
        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade4 { get; set; }
        [Range(0, 5, ErrorMessage = "Оценка должна быть от 0 до 5")]
        public int Grade5 { get; set; }

        public decimal AverageScore { get; set; }  // Авто-вычисляется (sum grades / 5)

        [ForeignKey("Specialty")]
        public int SpecialtyId { get; set; }
        public Specialty Specialty { get; set; }  // Навигация

        public User User { get; set; }  // One-to-one с User
        public void CalculateAverageScore()
        {
            AverageScore = (decimal)(Grade1 + Grade2 + Grade3 + Grade4 + Grade5) / 5;
        }
    }

}