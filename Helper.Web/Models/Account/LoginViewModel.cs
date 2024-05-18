using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Account;

public class LoginViewModel
{
    [Required(ErrorMessage = "Це поле обов'язкове")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Це поле обов'язкове")]
    public string Password { get; set; }
}
