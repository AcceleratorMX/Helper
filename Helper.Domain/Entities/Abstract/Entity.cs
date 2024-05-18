namespace Helper.Domain.Entities.Abstract;

public abstract class Entity<TPrimaryKey>
{
    public TPrimaryKey? Id { get; set; }
}
