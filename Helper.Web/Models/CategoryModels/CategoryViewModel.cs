using System.ComponentModel.DataAnnotations;
using Helper.Domain.Entities;

namespace Helper.Web.Models.CategoryModels;

public class CategoryViewModel
{
    public int Id { get; set; }
    
    [Display(Name = "Назва")]
    [Required(ErrorMessage = "Введіть назву!")]
    public string? Title { get; set; }
    
    [Required(ErrorMessage = "Введіть опис!")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }
    
    public int JobId { get; set; }
    
    public Job? Job { get; set; }

    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
}