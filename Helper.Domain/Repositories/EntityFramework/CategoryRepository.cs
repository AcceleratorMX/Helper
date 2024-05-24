using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain.Repositories.EntityFramework;

public class CategoryRepository(HelperDbContext context) : IRepository<Category, int>
{
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await context.Categories.ToListAsync();
    }

    public async Task<Category> GetByIdAsync(int id)
    {
        return await context.Categories.FindAsync(id) ??
               throw new Exception($"Category with id {id} not found");
    }

    public async Task CreateAsync(Category entity)
    {
        await context.Categories.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category entity)
    {
        context.Categories.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category != null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }
}