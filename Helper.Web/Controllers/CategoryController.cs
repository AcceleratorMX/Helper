using System.Security.Claims;
using Helper.Domain.Entities;
using Helper.Domain.Repositories.Abstract;
using Helper.Domain.Service;
using Helper.Web.Models.CategoryModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helper.Web.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category, int> _categoryRepository;
        private readonly IRepository<Job, int> _jobRepository;
        private readonly ValidationService _validationService;

        public CategoryController(IRepository<Category, int> categoryRepository, ValidationService validationService, IRepository<Job, int> jobRepository)
        {
            _categoryRepository = categoryRepository;
            _validationService = validationService;
            _jobRepository = jobRepository;
        }

        public async Task<IActionResult> CategoryEditor()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var model = new CategoryViewModel
            {
                Categories = categories
            };

            return View(model);
        }
        
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("CategoryEditor", model);
            }
            

            var category = new Category
            {
                Title = model.Title!,
                Description = model.Description!
            };

            await _categoryRepository.CreateAsync(category);
            return RedirectToAction("CategoryEditor");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var jobs = await _jobRepository.GetAllAsync();
            foreach (var job in jobs)
            {
                if (job.CategoryId == id)
                {
                    job.CategoryId = GetDefaultCategoryId();
                    await _jobRepository.UpdateAsync(job);
                }
            }

            await _categoryRepository.DeleteAsync(id);
            return RedirectToAction("CategoryEditor");
        }

        private int GetDefaultCategoryId()
        {
            var defaultCategory = _categoryRepository.GetAllAsync().Result.FirstOrDefault(c => c.Title == "Без категорії");
            return defaultCategory?.Id ?? throw new InvalidOperationException("Default category not found");
        }
    }
}
