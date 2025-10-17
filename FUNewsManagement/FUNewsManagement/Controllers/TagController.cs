using BusinessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly INewsService _newsService;

        public TagController(ITagService tagService, INewsService newsService)
        {
            _tagService = tagService;
            _newsService = newsService;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var all = await _tagService.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(q)) all = all.Where(t => (t.TagName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
            // compute which tags are used by any article
            var usedTagIds = (await _newsService.GetAllAsync())
                .Where(n => n.Tags != null)
                .SelectMany(n => n.Tags.Select(t => t.TagId))
                .Distinct()
                .ToHashSet();

            ViewBag.UsedTagIds = usedTagIds;
            return View(all);
        }

        public IActionResult Create() => View();

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(Tag model)
        {
            if (!ModelState.IsValid) return View(model);
            var exists = (await _tagService.GetAllAsync()).Any(t => string.Equals(t.TagName, model.TagName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                ModelState.AddModelError("TagName", "Tag name already exists");
                return View(model);
            }
            await _tagService.AddAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var t = await _tagService.GetByIdAsync(id);
            return t == null ? NotFound() : View(t);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> Edit(Tag model)
        {
            if (!ModelState.IsValid) return View(model);
            // check duplicate excluding self
            var exists = (await _tagService.GetAllAsync()).Any(t => t.TagId != model.TagId && string.Equals(t.TagName, model.TagName, StringComparison.OrdinalIgnoreCase));
            if (exists)
            {
                ModelState.AddModelError("TagName", "Tag name already exists");
                return View(model);
            }
            await _tagService.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            // allow service to remove junction rows first and then delete the tag
            await _tagService.DeleteAsync(id);
            TempData["Success"] = "Tag deleted";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var t = await _tagService.GetByIdAsync(id);
            if (t == null) return NotFound();
            // get articles using this tag
            var articles = (await _newsService.GetAllAsync()).Where(n => n.Tags != null && n.Tags.Any(tt => tt.TagId == id));
            ViewBag.Articles = articles;
            return View(t);
        }
    }
}
