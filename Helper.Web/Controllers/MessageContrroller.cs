using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.MessageModels;
using Microsoft.AspNetCore.Authorization;

namespace Helper.Web.Controllers;

[Authorize]
public class MessageController(
    IRepository<Message, long> messageRepository,
    IRepository<Job, int> jobRepository,
    IRepository<User, Guid> userRepository,
    ValidationService validationService)
    : Controller
{
    private const int Limit = 5;
    
    public async Task<IActionResult> CreateMessage(int jobId)
    {
        var job = await jobRepository.GetByIdAsync(jobId);

        var creator = await userRepository.GetByIdAsync(job.CreatorId!.Value);

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

        if (!ModelState.IsValid || validationService.IsHasMoreSpaces(model.Text, Limit))
        {
            ModelState.AddModelError("Text", $"Повідомлення не повинно містити більше {Limit} підряд!");
            return View(model);
        }

        var job = await jobRepository.GetByIdAsync(model.JobId);

        var message = new Message
        {
            JobId = model.JobId,
            Text = model.Text.Trim(),
            CreatedAt = DateTime.Now,
            SenderId = activeUserId,
            ReceiverId = job.CreatorId!.Value
        };

        await messageRepository.CreateAsync(message);
        
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveMessage(long messageId)
    {
        var message = await messageRepository.GetByIdAsync(messageId);

        message.Status = MessageStatuses.Approved.ToString();
        await messageRepository.UpdateAsync(message);

        var job = await jobRepository.GetByIdAsync(message.JobId);

        job.Status = JobStatuses.InProgress.ToString();
        job.AssigneeId = message.SenderId;
        await jobRepository.UpdateAsync(job);

        var user = await userRepository.GetByIdAsync(message.SenderId!.Value);
        
        user.AcceptedJobs++;
        await userRepository.UpdateAsync(user);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectMessage(long messageId)
    {
        var message = await messageRepository.GetByIdAsync(messageId);
        
        message.Status = MessageStatuses.Rejected.ToString();
        await messageRepository.UpdateAsync(message);
        
        return RedirectToAction("Index", "Home");
    }
}