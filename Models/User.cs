using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public long TelegramId { get; set; }
    [Required]
    public string Username { get; set; } // Новый: генерируемый логин, типа "applicant_123"

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    public string Role { get; set; } // "Applicant", etc.

    public bool IsConfirmed { get; set; } = false; // Подтверждена ли регистрация через Telegram

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}