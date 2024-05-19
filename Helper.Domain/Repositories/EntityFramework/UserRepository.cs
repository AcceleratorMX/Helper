using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain.Repositories.EntityFramework;

public class UserRepository(HelperDbContext context) : IRepository<User, Guid>
{
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await context.Users.ToListAsync();
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        return await context.Users.FindAsync(id) ?? 
               throw new Exception($"User with id {id} not found");
    }

    public async Task CreateAsync(User entity)
    {
        entity.LastLoginDate = DateTime.UtcNow;
        
        await context.Users.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User entity)
    {
        entity.LastLoginDate = DateTime.UtcNow;
        context.Users.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }
}