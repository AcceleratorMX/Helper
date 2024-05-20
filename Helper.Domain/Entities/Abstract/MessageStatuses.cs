using System.ComponentModel.DataAnnotations;

namespace Helper.Domain.Entities.Abstract;

public enum MessageStatuses
{
    [Display(Name = "Надіслано")]
    Sent,
    [Display(Name = "Підтверджено")]
    Approved,
    [Display(Name = "Відхилено")]
    Rejected
}