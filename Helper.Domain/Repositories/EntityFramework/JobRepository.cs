using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain.Repositories.EntityFramework;

public class JobRepository(HelperDbContext context) : IRepository<Job, int>
{
    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await context.Jobs.Include(job => job.Category).ToListAsync();
    }
    
    public async Task<Job> GetByIdAsync(int id)
    {
        var job = await context.Jobs.Include(job => job.Category).FirstOrDefaultAsync(job => job.Id == id);
        if (job == null)
        {
            throw new Exception($"JobModels with id {id} not found");
        }
        return job;
    }


    public async Task CreateAsync(Job entity)
    {
        await context.Jobs.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job entity)
    {
        context.Jobs.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = await context.Jobs.FindAsync(id);
        if (job != null)
        {
            context.Jobs.Remove(job);
            await context.SaveChangesAsync();
        }
    }
}