using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Helper.Web.Controllers;

[Authorize]
public class JobController : Controller
{
    private readonly IRepository<Job, int> _jobRepository;
    private readonly IRepository<Category, int> _categoryRepository;
    private readonly IRepository<User, Guid> _userRepository;

    public JobController(IRepository<Job, int> jobRepository, IRepository<Category, int> categoryRepository, IRepository<User, Guid> userRepository)
    {
        _jobRepository = jobRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> CreateJob()
    {
        var categories = await _categoryRepository.GetAllAsync();
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
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Title");
            return View("CreateJob", job);
        }
        
        job.CreatorId = Guid.Parse(activeUserId!);
        
        await _jobRepository.CreateAsync(job);
        
        var user = await _userRepository.GetByIdAsync(Guid.Parse(activeUserId!));
        user.CreatedJobs++;
        await _userRepository.UpdateAsync(user);
        
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Edit()
    {
        throw new NotImplementedException();
    }
}