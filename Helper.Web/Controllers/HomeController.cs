using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Repositories.Abstract;
using Helper.Web.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Job, int> _jobRepository;

    public HomeController(ILogger<HomeController> logger, IRepository<Job, int> jobRepository)
    {
        _logger = logger;
        _jobRepository = jobRepository;
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
        }

        return View(model);
    }

}