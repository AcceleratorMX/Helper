using Helper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain;

public class HelperDbContext(DbContextOptions<HelperDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Category> Categories { get; set; }



}
