using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ім'я користувача!")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Від {2} до {1} символів!")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Введіть пароль!")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Від {2} до {1} символів!")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Повторіть пароль!")]
    [Compare("RepeatPassword", ErrorMessage = "Паролі не співпадають")]
    public string RepeatPassword { get; set; } = null!;
}