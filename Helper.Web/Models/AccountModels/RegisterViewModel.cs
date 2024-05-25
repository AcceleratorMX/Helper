using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.AccountModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть ім'я користувача!")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "Від {2} до {1} символів!")]
    [RegularExpression(@"^\S*$", ErrorMessage = "Ім'я користувача не повинно містити пробіли")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Введіть пароль!")]
    [StringLength(30, MinimumLength = 6, ErrorMessage = "Від {2} до {1} символів!")]
    [RegularExpression(@"^\S*$", ErrorMessage = "Пароль не повинен містити пробіли")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Повторіть пароль!")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string RepeatPassword { get; set; } = null!;
}
