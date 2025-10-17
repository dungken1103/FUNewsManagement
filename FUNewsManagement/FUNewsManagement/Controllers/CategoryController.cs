using BusinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly INewsService _newsService;

        public CategoryController(ICategoryService categoryService, INewsService newsService)
        {
            _categoryService = categoryService;
            _newsService = newsService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? q)
        {
            var cats = await _categoryService.SearchAsync(q);

                var articles = (await _newsService.GetAllAsync()).ToList();
                var articlesByCategory = articles
                    .Where(a => a.CategoryId.HasValue)
                    .GroupBy(a => a.CategoryId.Value)
                    .ToDictionary(g => g.Key, g => g.Take(3).ToList());

            ViewBag.ArticlesByCategory = articlesByCategory;
            return View(cats);
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Create()
        {
            return View();
        }

        // Return partial view for modal
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CreateModal()
        {
            var cats = await _categoryService.GetAllAsync();
            ViewBag.ParentCategories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cats, "CategoryId", "CategoryName");
            return PartialView("_CreateEdit", new Category { IsActive = false });
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(Category model)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.AddAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Edit(short id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            return cat == null ? NotFound() : PartialView("_CreateEdit", cat);
        }

        // Return partial view for modal (for consistency)
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> EditModal(short id)
        {
            var cat = await _categoryService.GetByIdAsync(id);
            if (cat == null) return NotFound();
            var cats = await _categoryService.GetAllAsync();
            ViewBag.ParentCategories = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(cats, "CategoryId", "CategoryName", cat.ParentCategoryId);
            return PartialView("_CreateEdit", cat);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.UpdateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
