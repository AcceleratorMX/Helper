using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;

namespace Helper.Domain.Service;

public class JobService(IRepository<Job, int> jobRepository,
    IRepository<User, Guid> userRepository,
    IRepository<Message, long> messageRepository)
{    
    
    public async Task DeleteJobAndRelatedMessages(int jobId)
    {

        var messages = await messageRepository.GetAllAsync();
        foreach (var message in messages)
        {
            if (message.JobId == jobId)
            {
                await messageRepository.DeleteAsync(message.Id);
            }
        }

        await jobRepository.DeleteAsync(jobId);
    }

}

