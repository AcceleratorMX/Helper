using System.ComponentModel.DataAnnotations;

namespace Helper.Domain.Entities.Abstract;

public enum JobStatuses
{
    [Display(Name = "Активне")]
    Active,
    [Display(Name = "Виконується")]
    InProgress,
    [Display(Name = "Завершено")]
    Completed,
    [Display(Name = "Відмінено")]
    Canceled
}
