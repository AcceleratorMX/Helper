using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.CategoryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers;

[Authorize]
public class CategoryController(
    IRepository<Category, int> categoryRepository,
    ValidationService validationService,
    IRepository<Job, int> jobRepository)
    : Controller
{
    public async Task<IActionResult> CategoryEditor()
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var categories = await categoryRepository.GetAllAsync();

        var model = new CategoryViewModel
        {
            Categories = categories
        };

        return View(model);
    }

    public async Task<IActionResult> CreateCategoryAsync(CategoryViewModel model)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        if (!ModelState.IsValid)
        {
            var categories = await categoryRepository.GetAllAsync();
            model.Categories = categories;
            return View("CategoryEditor", model);
        }
        
        
        var existingCategory = (await categoryRepository.GetAllAsync())
            .FirstOrDefault(c => c.Title.Equals(model.Title, StringComparison.OrdinalIgnoreCase));

        if (existingCategory != null)
        {
            ModelState.AddModelError("Title", "Категорія з таким ім'ям вже існує.");
            var categories = await categoryRepository.GetAllAsync();
            model.Categories = categories;
            return View("CategoryEditor", model);
        }

        var category = new Category
        {
            Title = model.Title!.Trim(),
            Description = model.Description!
        };

        await categoryRepository.CreateAsync(category);
        return RedirectToAction("CategoryEditor");
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!validationService.IsAdmin(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
            return RedirectToAction("Index", "Home");

        var jobs = await jobRepository.GetAllAsync();
        foreach (var job in jobs)
        {
            if (job.CategoryId == id)
            {
                job.CategoryId = GetDefaultCategoryId();
                await jobRepository.UpdateAsync(job);
            }
        }

        await categoryRepository.DeleteAsync(id);
        return RedirectToAction("CategoryEditor");
    }

    private int GetDefaultCategoryId()
    {
        var defaultCategory = categoryRepository.GetAllAsync().Result.FirstOrDefault(c => c.Title == "Без категорії");
        return defaultCategory?.Id ?? throw new InvalidOperationException("Default category not found");
    }
}