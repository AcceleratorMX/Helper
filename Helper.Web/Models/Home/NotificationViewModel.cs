using System.ComponentModel.DataAnnotations;

namespace Helper.Web.Models.Home;

public class NotificationViewModel
{
    public long MessageId { get; set; }
    
    [Display(Name = "Вміст")]
    public string Content { get; set; } = null!;
    
    [Display(Name = "Відправник")]
    public string SenderName { get; set; } = null!;
    
    [Display(Name = "Назва завдання")]
    public string JobTitle { get; set; } = null!;
    
    [Display(Name = "Дата відправки")]
    public DateTime SentAt { get; set; }
    
    [Display(Name = "Статус")]
    public string Status { get; set; } = null!;
    
    
}