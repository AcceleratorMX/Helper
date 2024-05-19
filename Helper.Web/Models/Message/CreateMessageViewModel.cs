using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Message;

public class CreateMessageViewModel
{
    public int JobId { get; set; }
    
    [Display(Name = "Назва завдання")]
    public string? JobTitle { get; set; }
    
    [Display(Name = "Створив")]
    public string? CreatorName { get; set; }
    
    [Display(Name = "Текст повідомлення")]
    [Required(ErrorMessage = "Введіть текст повідомлення")]
    public string Text { get; set; } = null!;
    
}