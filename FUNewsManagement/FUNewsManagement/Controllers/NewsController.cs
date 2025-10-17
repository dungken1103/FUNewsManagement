using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    public class NewsController : Controller
    {
        protected readonly INewsService _newsService;
        protected readonly ICategoryService _categoryService;
        protected readonly ITagService _tagService;

        public NewsController(INewsService newsService, ICategoryService categoryService, ITagService tagService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? q, short? categoryId, DateTime? from, DateTime? to)
        {
            var list = await _newsService.SearchAsync(q, categoryId, from, to);
            // ensure categories and tags available for filters
            ViewBag.Categories = await _categoryService.GetAllAsync() ?? new List<DataAccessLayer.Models.Category>();
            ViewBag.Tags = await _tagService.GetAllAsync() ?? new List<DataAccessLayer.Models.Tag>();
            return View(list);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            var article = await _newsService.GetByIdAsync(id);
            if (article == null || article.NewsStatus != true) return NotFound();
            // get related (up to 3)
            var related = await _newsService.GetRelatedAsync(id, (short)(article.CategoryId ?? 0));
            ViewBag.Related = related;
            return View(article);
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Tags = await _tagService.GetAllAsync();
            return View();
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(NewsArticle model, int[] selectedTags)
        {
            Console.WriteLine(">>> Create News CALLED <<<");
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            Console.WriteLine($"UserId from session: {userId}");

            if (userId == 0)
            {
                return Unauthorized();
            }
            if (!ModelState.IsValid)
            {
                model.Tags = new List<Tag>();
                foreach (var tId in selectedTags ?? Array.Empty<int>())
                {
                    var tag = await _tagService.GetByIdAsync(tId);
                    if (tag != null)
                        model.Tags.Add(tag);
                }

                await _newsService.CreateAsync(model, (short)userId);
                return RedirectToAction("Index");
            }

            // Invalid model - re-populate selects and return view for correction
            ViewBag.Categories = await _category_service_GetAllSafe();
            ViewBag.Tags = await _tagService.GetAllAsync();
            return View(model);
        }

        // helper to safely get categories (keeps callsites small and avoids null)
        private async Task<IEnumerable<DataAccessLayer.Models.Category>> _category_service_GetAllSafe()
        {
            return await _categoryService.GetAllAsync() ?? new List<DataAccessLayer.Models.Category>();
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Edit(string id)
        {
            var article = await _newsService.GetByIdAsync(id);
            if (article == null) return NotFound();
            ViewBag.Categories = await _categoryService.GetAllAsync();
            ViewBag.Tags = await _tagService.GetAllAsync();
            return View(article);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Edit(NewsArticle model, int[] selectedTags)
        {
            Console.WriteLine(">>> Edit News CALLED <<<");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ViewBag.Tags = await _tagService.GetAllAsync();
                return View(model);
            }

            // Lấy bài viết từ DB
            var news = await _newsService.GetByIdAsync(model.NewsArticleId);
            if (news == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            news.NewsTitle = model.NewsTitle;
            news.Headline = model.Headline;
            news.NewsContent = model.NewsContent;
            news.CategoryId = model.CategoryId;
            news.UpdatedById = (short)userId;
            news.NewsStatus = model.NewsStatus;
            news.ModifiedDate = DateTime.Now;

            // Cập nhật Tags
            news.Tags.Clear(); // Xóa các tag cũ
            if (selectedTags != null)
            {
                foreach (var tId in selectedTags)
                {
                    var tag = await _tagService.GetByIdAsync(tId); // lấy tag từ DB
                    if (tag != null)
                        news.Tags.Add(tag);
                }
            }

            await _newsService.UpdateAsync(news);
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _newsService.DeleteAsync(id);
                TempData["Success"] = "Article deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }




        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Duplicate(string id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            await _newsService.DuplicateAsync(id, (short)userId);
            return RedirectToAction("Index");
        }
    }
}
