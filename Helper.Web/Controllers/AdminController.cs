using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Service;

namespace Helper.Web.Controllers;

[Authorize]
public class AdminController(
    IRepository<Job, int> jobRepository,
    IRepository<Category, int> categoryRepository,
    IRepository<User, Guid> userRepository,
    IRepository<Message, long> messageRepository,
    ValidationService validationService)
    : Controller
{
    private readonly IRepository<Category, int> _categoryRepository = categoryRepository;
    
    private const string AdminUsername = "admin";

    public async Task<IActionResult> AdminPage(string? search)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var usersWithoutAdmin = await GetUsersWithoutAdmin();

        if (!string.IsNullOrEmpty(search))
        {
            usersWithoutAdmin = SearchUser(usersWithoutAdmin, search);
        }

        return View(usersWithoutAdmin);
    }

    private async Task<List<User>> GetUsersWithoutAdmin()
    {
       return (await userRepository.GetAllAsync())
           .Where(u => u.Username != AdminUsername)
           .OrderBy(u => u.Username)
           .ToList();
    } 
    
    private List<User> SearchUser(List<User> users, string search)
    {
        var searchedUser = users.FirstOrDefault(u => u.Username.Equals(search, StringComparison.CurrentCultureIgnoreCase));
        if (searchedUser != null)
        {
            return new List<User> { searchedUser };
        }
        else
        {
            ViewData.ModelState.AddModelError(string.Empty,"Користувача з таким ім'ям не знайдено: " + search);
            return users;
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var userJobs = (await jobRepository.GetAllAsync())
            .Where(job => job.AssigneeId == userId || job.CreatorId == userId)
            .ToList();

        foreach (var job in userJobs)
        {
            if (job.AssigneeId == userId)
            {
                job.AssigneeId = null;
            }
            else if (job.CreatorId == userId)
            {
                job.CreatorId = null;
            }

            await jobRepository.UpdateAsync(job);
        }

        var userMessages = (await messageRepository.GetAllAsync())
            .Where(message => message.ReceiverId == userId || message.SenderId == userId)
            .ToList();

        foreach (var message in userMessages)
        {
            if (message.ReceiverId == userId)
            {
                message.ReceiverId = null;
            }
            else if (message.SenderId == userId)
            {
                message.SenderId = null;
            }

            await messageRepository.UpdateAsync(message);
        }

        await userRepository.DeleteAsync(userId);
        return RedirectToAction(nameof(AdminPage));
    }

    public async Task<IActionResult> AllUsersPage(Guid userId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var user = await userRepository.GetByIdAsync(userId);
        var jobs = await GetJobs(userId);

        ViewBag.UserName = user.Username;
        return View(jobs);
    }

    private async Task<List<Job>> GetJobs(Guid userId)
    {
        return (await jobRepository.GetAllAsync())
            .Where(t => t.AssigneeId == userId || t.CreatorId == userId)
            .OrderBy(t => t.Status == JobStatuses.InProgress.ToString())
            .ThenBy(t => t.Status == JobStatuses.Active.ToString())  
            .ThenBy(t => t.Status == JobStatuses.Completed.ToString())
            .ToList();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUserJob(int jobId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var job = await jobRepository.GetByIdAsync(jobId);

        var userId = job.AssigneeId ?? job.CreatorId;
        await jobRepository.DeleteAsync(jobId);
        return RedirectToAction(nameof(AllUsersPage), new { userId });
    }
}