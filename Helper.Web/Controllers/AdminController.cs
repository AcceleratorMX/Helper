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

    public async Task<IActionResult> AdminPage(string? search)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var usersWithoutAdmin = await GetUsersWithoutAdminAsync();

        if (!string.IsNullOrEmpty(search))
        {
            usersWithoutAdmin = SearchUser(usersWithoutAdmin, search);
        }

        return View(usersWithoutAdmin);
    }

    private async Task<List<User>> GetUsersWithoutAdminAsync()
    {
        return (await userRepository.GetAllAsync())
            .Where(u => !validationService.IsAdmin(u.Id.ToString()))
            .OrderBy(u => u.Username)
            .ToList();
    }

    private List<User> SearchUser(List<User> users, string search)
    {
        var searchedUser =
            users.FirstOrDefault(u => u.Username.Equals(search, StringComparison.CurrentCultureIgnoreCase));
        if (searchedUser != null)
        {
            return new List<User> { searchedUser };
        }
        else
        {
            ViewData.ModelState.AddModelError(string.Empty, "Користувача з таким ім'ям не знайдено: " + search);
            return users;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserAsync(Guid userId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");
        
        await DeleteUserRelatedDataAsync(userId);
        await userRepository.DeleteAsync(userId);

        return RedirectToAction(nameof(AdminPage));
    }

    private async Task DeleteUserRelatedDataAsync(Guid userId)
    {
        await DeleteUserJobsAsync(userId);
        await DeleteUserMessagesAsync(userId);
    }

    private async Task DeleteUserJobsAsync(Guid userId)
    {
        var userJobs = (await jobRepository.GetAllAsync())
            .Where(job => job.AssigneeId == userId || job.CreatorId == userId)
            .ToList();

        foreach (var job in userJobs)
        {
            job.Status = JobStatuses.Active.ToString();
            
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

        await GetUpdatedJobsAsync();
    }

    private async Task GetUpdatedJobsAsync()
    {
        var updatedJobs = (await jobRepository.GetAllAsync())
            .Where(j => j.AssigneeId == null && j.CreatorId == null)
            .ToList();

        foreach (var updatedJob in updatedJobs)
        {
            await jobRepository.DeleteAsync(updatedJob.Id);
        }
    }

    private async Task DeleteUserMessagesAsync(Guid userId)
    {
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

        await GetUpdatedMessagesAsync();
    }

    private async Task GetUpdatedMessagesAsync()
    {
        var updatedMessages = (await messageRepository.GetAllAsync())
            .Where(message => message.ReceiverId == null || message.SenderId == null)
            .ToList();
        foreach (var updatedMessage in updatedMessages)
        {
            await messageRepository.DeleteAsync(updatedMessage.Id);
        }
    }

    public async Task<IActionResult> AllUserJobsPage(Guid userId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var user = await userRepository.GetByIdAsync(userId);
        var jobs = await GetJobsAsync(userId);

        ViewBag.UserName = user.Username;
        return View(jobs);
    }

    private async Task<List<Job>> GetJobsAsync(Guid userId)
    {
        return (await jobRepository.GetAllAsync())
            .Where(t => t.AssigneeId == userId || t.CreatorId == userId)
            .OrderBy(t => t.Status == JobStatuses.InProgress.ToString())
            .ThenBy(t => t.Status == JobStatuses.Active.ToString())
            .ThenBy(t => t.Status == JobStatuses.Completed.ToString())
            .ToList();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserJobAsync(int jobId)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var job = await jobRepository.GetByIdAsync(jobId);

        var userId = job.AssigneeId ?? job.CreatorId;
        await jobRepository.DeleteAsync(jobId);
        return RedirectToAction(nameof(AllUserJobsPage), new { userId });
    }
}