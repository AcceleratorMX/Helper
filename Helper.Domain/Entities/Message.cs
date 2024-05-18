using Helper.Domain.Entities.Abstract;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Helper.Domain.Entities;

public class Message : Entity<long>
{
    [Display(Name = "Текст повідомлення")]
    [Required(ErrorMessage = "Введіть текст повідомлення")]
    public string Text { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public int JobId { get; set; }

    [ForeignKey(nameof(JobId))]
    public Job? Job { get; set; }

    public Guid SenderId { get; set; }

    [ForeignKey(nameof(SenderId))]
    public User? Sender { get; set; }

    public Guid ReceiverId { get; set; }

    [ForeignKey(nameof(ReceiverId))]
    public User? Receiver { get; set; }
}
