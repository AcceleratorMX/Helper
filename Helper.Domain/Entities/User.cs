using Helper.Domain.Entities.Abstract;
using System.ComponentModel.DataAnnotations;

namespace Helper.Domain.Entities;

public class User : Entity<Guid>
{
    [Required(ErrorMessage = "Введіть ім'я користувача")]
    [Display(Name = "Ім'я користувача")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Введіть пароль")]
    [Display(Name = "Пароль")]
    public string? Password { get; set; }

    [Display(Name = "Електронна пошта")]
    public string? Email { get; set; } = string.Empty;

    [Display(Name = "Місто")]
    public string? City { get; set; } = string.Empty;

    [Display(Name = "Дата реєстрації")]
    public DateTime RegisterDate { get; init; } = DateTime.UtcNow;

    [Display(Name = "Дата останнього входу")]
    public DateTime LastLoginDate { get; set; }

    [Display(Name = "Шлях до фото профілю")]
    public string ProfilePicturePath { get; set; } = string.Empty;
}
