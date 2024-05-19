using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Web.Models.Message;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Helper.Web.Controllers;

[Authorize]
public class MessageController : Controller
{
    private readonly IRepository<Message, long> _messageRepository;
    private readonly IRepository<Job, int> _jobRepository;
    private readonly IRepository<User, Guid> _userRepository;

    public MessageController(IRepository<Message, long> messageRepository, IRepository<Job, int> jobRepository,
        IRepository<User, Guid> userRepository)
    {
        _messageRepository = messageRepository;
        _jobRepository = jobRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> CreateMessage(int jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);

        var creator = await _userRepository.GetByIdAsync(job.CreatorId);

        var model = new CreateMessageViewModel
        {
            JobId = job.Id,
            JobTitle = job.Title,
            CreatorName = creator.Username
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMessage(CreateMessageViewModel model)
    {
        var activeUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var job = await _jobRepository.GetByIdAsync(model.JobId);

        var message = new Message
        {
            JobId = model.JobId,
            Text = model.Text,
            CreatedAt = DateTime.Now,
            SenderId = activeUserId,
            ReceiverId = job.CreatorId,
        };

        await _messageRepository.CreateAsync(message);

        return RedirectToAction("Index", "Home");
    }
}