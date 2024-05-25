using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.AccountModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть ім'я!")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Введіть пароль!")]
    public string Password { get; set; } = null!;
}