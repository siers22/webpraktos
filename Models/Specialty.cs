using System.ComponentModel.DataAnnotations;

namespace PRAKTOSWEBAPI.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }  // Название специальности, e.g. "Программирование"
    }
}