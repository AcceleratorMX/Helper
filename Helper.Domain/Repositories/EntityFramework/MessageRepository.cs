using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Helper.Domain.Repositories.EntityFramework;

public class MessageRepository(HelperDbContext context) : IRepository<Message, long>
{
    public async Task<IEnumerable<Message>> GetAllAsync()
    {
        return await context.Messages.ToListAsync();
    }

    public async Task<Message> GetByIdAsync(long id)
    {
        return await context.Messages.FindAsync(id) ??
               throw new Exception($"Message with id {id} not found");
    }

    public async Task CreateAsync(Message entity)
    {
        if (entity.SenderId == entity.ReceiverId)
        {
            throw new Exception("Sender and receiver cannot be the same");
        }

        await context.Messages.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Message entity)
    {
        var message = await context.Messages.FindAsync(entity.Id);
        context.Messages.Update(message!);
        await context.SaveChangesAsync();
    }

    public Task DeleteAsync(long id)
    {
        var message = context.Messages.Find(id);
        context.Messages.Remove(message ?? throw new Exception($"Message with id {id} not found"));
        return context.SaveChangesAsync();
    }
}