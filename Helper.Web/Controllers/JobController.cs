using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Entities.Abstract;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.JobModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Helper.Web.Controllers;

[Authorize]
public class JobController(
    IRepository<Job, int> jobRepository,
    IRepository<Category, int> categoryRepository,
    IRepository<User, Guid> userRepository,
    ValidationService validationService)
    : Controller
{
    private const int Limit = 5;
    
    [HttpGet]
    public async Task<IActionResult> CreateJob()
    {
        await GetCategories();
        return View(new CreateEditJobViewModel());
    }

    private async Task GetCategories()
    {
        var categories = (await categoryRepository.GetAllAsync())
            .Where(c => c.Id != 1 && c.Id != 2)
            .ToList();
        ViewBag.Categories = new SelectList(categories, "Id", "Title");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobAsync(CreateEditJobViewModel model)
    {
        var activeUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!ModelState.IsValid )
        {
            await GetCategories();
            return View("CreateJob", model);
        }
        
        if (WhiteSpacesSpamCheck(model, out var result)) return result;

        var job = new Job
        {
            Title = model.Title!.Trim(),
            Description = model.Description!.Trim(),
            Location = model.Location!.Trim(),
            CategoryId = model.CategoryId,
            CreatorId = Guid.Parse(activeUserId!)
        };
        
        await jobRepository.CreateAsync(job);

        var user = await userRepository.GetByIdAsync(Guid.Parse(activeUserId!));
        user.CreatedJobs++;
        await userRepository.UpdateAsync(user);

        return RedirectToAction("Index", "Home");
    }

    private bool WhiteSpacesSpamCheck(CreateEditJobViewModel model, out IActionResult result)
    {
        if(validationService.IsHasMoreSpaces(model.Title!, Limit))
        {
            ModelState.AddModelError("Title", $"Заголовок не повинен містити більше {Limit} пробілів підряд!");
            {
                result = View("CreateJob", model);
                return true;
            }
        }
        if(validationService.IsHasMoreSpaces(model.Description!, Limit))
        {
            ModelState.AddModelError("Description", $"Опис не повинен містити більше {Limit} пробілів підряд!");
            {
                result = View("CreateJob", model);
                return true;
            }
        }
        if(validationService.IsHasMoreSpaces(model.Location!, Limit))
        {
            ModelState.AddModelError("Location", $"Місцезнаходження не повинно містити більше {Limit} пробілів підряд!");
            {
                result = View("CreateJob", model);
                return true;
            }
        }

        result = null!;
        return false;
    }


    [HttpGet]
    public async Task<IActionResult> EditJob(int id)
    {
        var job = await jobRepository.GetByIdAsync(id);

        await GetCategories();

        var viewModel = new CreateEditJobViewModel
        {
            Id = job.Id,
            Title = job.Title.Trim(),
            Description = job.Description.Trim(),
            Location = job.Location.Trim(),
            CategoryId = job.CategoryId
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(CreateEditJobViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await GetCategories();
            return View(model);
        }

        if (WhiteSpacesSpamCheck(model, out var result)) return result;

        var job = await jobRepository.GetByIdAsync(model.Id);

        job.Title = model.Title!.Trim();
        job.Description = model.Description!.Trim();
        job.Location = model.Location!.Trim();
        job.CategoryId = model.CategoryId;

        await jobRepository.UpdateAsync(job);

        return RedirectToAction("Index", "Home");
    }
    
    
    [HttpPost]
    public async Task<IActionResult> DeleteJob(int jobId)
    {
        var job = await jobRepository.GetByIdAsync(jobId);

        var userId = job.AssigneeId ?? job.CreatorId;
        await jobRepository.DeleteAsync(jobId);
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCompletion(int id)
    {
        var job = await jobRepository.GetByIdAsync(id);

        job.Status = JobStatuses.Completed.ToString();
        var user = await userRepository.GetByIdAsync(job.AssigneeId!.Value);
        user.CompletedJobs++;
        await userRepository.UpdateAsync(user);

        await jobRepository.UpdateAsync(job);

        return RedirectToAction("AllJobs", "Job");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelJob(int id)
    {
        var job = await jobRepository.GetByIdAsync(id);

        var user = await userRepository.GetByIdAsync(job.AssigneeId!.Value);
        user.FailedJobs++;
        await userRepository.UpdateAsync(user);
        job.Status = JobStatuses.Active.ToString();
        job.AssigneeId = null;
        await jobRepository.UpdateAsync(job);

        return RedirectToAction("AllJobs", "Job");
    }

    public async Task<IActionResult> AllJobs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var createdJobs = await GetCreatedJobs(userId);
        var assignedJobs = await GetAssignedJobs(userId);

        ViewBag.CreatedJobs = createdJobs;
        ViewBag.AssignedJobs = assignedJobs;

        return View();
    }

    private async Task<List<Job>> GetCreatedJobs(string? userId)
    {
        var createdJobs = (await jobRepository.GetAllAsync())
            .Where(j => j.CreatorId == Guid.Parse(userId!))
            .OrderByDescending(j => j.Status == JobStatuses.InProgress.ToString())
            .ThenBy(j => j.Status == JobStatuses.Completed.ToString())
            .ThenBy(j => j.Status == JobStatuses.Active.ToString())
            .ToList();
        return createdJobs;
    }

    private async Task<List<Job>> GetAssignedJobs(string? userId)
    {
        var assignedJobs = (await jobRepository.GetAllAsync())
            .Where(j => j.AssigneeId == Guid.Parse(userId!))
            .OrderByDescending(j => j.Status == JobStatuses.InProgress.ToString())
            .ThenBy(j => j.Status == JobStatuses.Completed.ToString())
            .ToList();
        return assignedJobs;
    }
}