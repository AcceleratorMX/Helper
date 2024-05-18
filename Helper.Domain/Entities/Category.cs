using Helper.Domain.Entities.Abstract;
using System.ComponentModel.DataAnnotations;

namespace Helper.Domain.Entities;

public class Category : Entity<int>
{
    [Display(Name = "Назва категорії")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Опис категорії")]
    public string Description { get; set; } = string.Empty;
}
