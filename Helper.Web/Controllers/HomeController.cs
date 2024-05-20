using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Repositories.Abstract;
using Helper.Web.Models.Home;
using Helper.Web.Models.Message;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Job, int> _jobRepository;
    private readonly IRepository<Message, long> _messageRepository;
    private readonly IRepository<User, Guid> _userRepository;

    public HomeController(ILogger<HomeController> logger, IRepository<Job, int> jobRepository, IRepository<Message, long> messageRepository, IRepository<User, Guid> userRepository)
    {
        _logger = logger;
        _jobRepository = jobRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            JobModel = new Job(),
            Jobs = (await _jobRepository.GetAllAsync())
                .Where(job => job.Status == JobStatuses.Active.ToString())
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
                .ToList()
                
        };

        if (User.Identity!.IsAuthenticated)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            ViewData["CurrentUserId"] = currentUserId;
            
            var notifications = await GetNotificationsAsync(currentUserId);
            ViewData["Notifications"] = notifications;
        }

        return View(model);
    }
    
    private async Task<List<NotificationViewModel>> GetNotificationsAsync(Guid userId)
    {
        return (await _messageRepository.GetAllAsync())
            .Where(m => m.ReceiverId == userId && m.Status == MessageStatuses.Sent.ToString())
            .Select(m => new NotificationViewModel
            {
                MessageId = m.Id,
                SenderName = _userRepository.GetByIdAsync(m.SenderId).Result.Username,
                JobTitle = _jobRepository.GetByIdAsync(m.JobId).Result.Title,
                SentAt = m.CreatedAt,
                Status = m.Status.ToString()
            })
            .Reverse()
            .ToList();
    }
    
    

}