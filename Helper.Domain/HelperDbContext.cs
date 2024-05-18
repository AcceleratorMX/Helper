using Helper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain;

public class HelperDbContext(DbContextOptions<HelperDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>()
            .HasOne(j => j.Creator)
            .WithMany()
            .HasForeignKey(j => j.CreatorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Job>()
            .HasOne(j => j.Assignee)
            .WithMany()
            .HasForeignKey(j => j.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);
    }

}
