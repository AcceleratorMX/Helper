using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Web.Models.Message;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Helper.Domain.Entities.Abstract;
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

    [HttpPost]
    public async Task<IActionResult> ApproveMessage(long messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);

        message.Status = MessageStatuses.Approved.ToString();
        await _messageRepository.UpdateAsync(message);

        var job = await _jobRepository.GetByIdAsync(message.JobId);

        job.Status = JobStatuses.InProgress.ToString();
        job.AssigneeId = message.SenderId;
        await _jobRepository.UpdateAsync(job);

        var user = await _userRepository.GetByIdAsync(message.SenderId);

        user.AcceptedJobs++;
        await _userRepository.UpdateAsync(user);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> RejectMessage(long messageId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);

        message.Status = MessageStatuses.Rejected.ToString();
        await _messageRepository.UpdateAsync(message);

        return RedirectToAction("Index", "Home");
    }
}