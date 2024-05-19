using Helper.Domain.Entities.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helper.Domain.Entities;

public class Job : Entity<int>
{
    [Display(Name = "Назва завдання")]
    [Required(ErrorMessage = "Вкажіть назву завдання!")]
    public string Title { get; set; } = null!;

    [Display(Name = "Опис завдання")]
    [Required(ErrorMessage = "Опишіть завдання!")]
    public string Description { get; set; } = null!;

    [Display(Name = "Місце виконання")]
    [Required(ErrorMessage = "Вкажіть місце виконання!")]
    public string Location { get; set; } = null!;

    [Display(Name = "Дата створення")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Дата завершення")]
    public DateTime? CompletedAt { get; set; }

    [Display(Name = "Статус")]
    public string Status { get; set; } = JobStatuses.Active.ToString();

    [Display(Name = "Створив")]
    public Guid CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public User? Creator { get; set; }

    [Display(Name = "Виконавець")]
    public Guid? AssigneeId { get; set; }

    [ForeignKey(nameof(AssigneeId))]
    public User? Assignee { get; set; }

    [Display(Name = "Категорія")]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public string JobPicturePath { get; set; } = string.Empty;

    public ICollection<Message>? Messages { get; set; }
}
