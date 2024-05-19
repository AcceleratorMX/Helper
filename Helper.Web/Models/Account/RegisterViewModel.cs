using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Account;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Це поле обов'язкове")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Це поле обов'язкове")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Це поле обов'язкове")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string RepeatPassword { get; set; } = null!;
}
