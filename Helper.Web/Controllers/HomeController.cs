using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers;

public class HomeController(
    IRepository<Job, int> jobRepository,
    IRepository<Message, long> messageRepository,
    IRepository<User, Guid> userRepository,
    IRepository<Category, int> categoryRepository,
    ValidationService validationService)
    : Controller
{
    public async Task<IActionResult> Index(string? category = null)
    {
        await GetCategories();
        var jobs = await GetJobs(category);

        ViewData["IsAdmin"] = validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)!
            : null;

        var model = new HomeViewModel
        {
            JobModel = new Job(),
            Jobs = jobs
        };

        await IsAuthenticatedUserAsync();

        return View(model);
    }

    private async Task IsAuthenticatedUserAsync()
    {
        if (User.Identity!.IsAuthenticated)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ViewData["CurrentUserId"] = currentUserId;

            var notifications = await GetNotificationsAsync(currentUserId);
            var notificationsCount = notifications.Count;
            ViewData["Notifications"] = notifications;
            ViewData["NotificationsCount"] = notificationsCount;
        }
    }

    private async Task GetCategories()
    {
        var categories = await categoryRepository.GetAllAsync();
        ViewData["Categories"] = categories
            .Where(c => c.Id != 1)
            .Select(c => c.Title)
            .ToList();
    }

    private async Task<List<Job>> GetJobs(string? category)
    {
        return (await jobRepository.GetAllAsync())
            .Where(job => job.Status == JobStatuses.Active.ToString() && job.CreatorId != null && (category == null ||
                category == "0" ||
                job.Category!.Title == category))
            .Select(job => new Job
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Category = job.Category,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                CreatorId = job.CreatorId
            })
            .Reverse()
            .ToList();
    }

    private async Task<List<NotificationViewModel>> GetNotificationsAsync(Guid userId)
    {
        return (await messageRepository.GetAllAsync())
            .Where(m => m.ReceiverId == userId && m.SenderId != null && m.Status == MessageStatuses.Sent.ToString())
            .Select(m => new NotificationViewModel
            {
                MessageId = m.Id,
                SenderName = userRepository.GetByIdAsync(m.SenderId!.Value).Result.Username,
                JobTitle = jobRepository.GetByIdAsync(m.JobId).Result.Title,
                Content = messageRepository.GetByIdAsync(m.Id).Result.Text,
                SentAt = m.CreatedAt,
                Status = m.Status.ToString()
            })
            .Reverse()
            .ToList();
    }
}