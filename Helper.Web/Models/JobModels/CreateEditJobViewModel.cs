using System.ComponentModel.DataAnnotations;
using Helper.Domain.Entities;

namespace Helper.Web.Models.JobModels;

public class CreateEditJobViewModel
{
    public int Id { get; set; }
    
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Від {2} до {1} символів!")]
    [Required(ErrorMessage = "Введіть назву!")]
    public string? Title { get; set; }
    
    [StringLength(500, MinimumLength = 15, ErrorMessage = "Від {2} до {1} символів!")]
    [Required(ErrorMessage = "Опишіть завдання!")]
    public string? Description { get; set; }
    
    [StringLength(50, MinimumLength = 4, ErrorMessage = "Від {2} до {1} символів!")]
    [Required(ErrorMessage = "Вкажіть місто!")]
    public string? Location { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Оберіть категорію!")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}