using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Web.Models.JobModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Helper.Web.Controllers;

[Authorize]
public class JobController(
    IRepository<Job, int> jobRepository,
    IRepository<Category, int> categoryRepository,
    IRepository<User, Guid> userRepository)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> CreateJob()
    {
        var categories = await categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Title");
        return View(new Job());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJobAsync(Job job)
    {
        var activeUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!ModelState.IsValid)
        {
            var categories = await categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Title");
            return View("CreateJob", job);
        }
        
        job.CreatorId = Guid.Parse(activeUserId!);
        
        await jobRepository.CreateAsync(job);
        
        var user = await userRepository.GetByIdAsync(Guid.Parse(activeUserId!));
        user.CreatedJobs++;
        await userRepository.UpdateAsync(user);
        
        return RedirectToAction("Index", "Home");
    }
    

    [HttpGet]
    public async Task<IActionResult> EditJob(int id)
    {
        var job = await jobRepository.GetByIdAsync(id);

        var categories = await categoryRepository.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Title");

        var viewModel = new EditJobViewModel
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            Location = job.Location,
            CategoryId = job.CategoryId
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(EditJobViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Title");
            return View(model);
        }

        var job = await jobRepository.GetByIdAsync(model.Id);

        job.Title = model.Title;
        job.Description = model.Description;
        job.Location = model.Location;
        job.CategoryId = model.CategoryId;
    
        await jobRepository.UpdateAsync(job);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteJob(EditJobViewModel model)
    {
        var job = await jobRepository.GetByIdAsync(model.Id);

        await jobRepository.DeleteAsync(model.Id);

        return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> AllJobs()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var createdJobs = (await jobRepository.GetAllAsync())
            .Where(j => j.CreatorId == Guid.Parse(userId!))
            .OrderByDescending(j => j.Status == "InProgress")
            .ThenBy(j => j.Status == "Active")
            .ToList();

        var assignedJobs = (await jobRepository.GetAllAsync())
            .Where(j => j.AssigneeId == Guid.Parse(userId!))
            .OrderByDescending(j => j.Status == "InProgress")
            .ThenBy(j => j.Status == "Completed")
            .ToList();

        ViewBag.CreatedJobs = createdJobs;
        ViewBag.AssignedJobs = assignedJobs;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCompletion(int id)
    {
        var job = await jobRepository.GetByIdAsync(id);

        job.Status = "Completed";
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
        job.Status = "Active";
        job.AssigneeId = null;
        await jobRepository.UpdateAsync(job);

        return RedirectToAction("AllJobs", "Job");
    }
}