using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.MessageModels;

public class CreateMessageViewModel
{
    public int JobId { get; set; }
    public string? JobTitle { get; set; }
    
    [Display(Name = "Створив")]
    public string? CreatorName { get; set; }
    
    [StringLength(50, MinimumLength = 20, ErrorMessage = "Від {2} до {1} символів!")]
    [Required(ErrorMessage = "Введіть тексть повідомлення!")]
    public string Text { get; set; } = null!;
    
}