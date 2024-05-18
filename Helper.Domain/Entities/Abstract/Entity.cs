using System.ComponentModel.DataAnnotations.Schema;

namespace Helper.Domain.Entities.Abstract;

public abstract class Entity<TPrimaryKey>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public TPrimaryKey? Id { get; set; }
}
