using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;

        public ReportController(INewsService newsService, ICategoryService categoryService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var all = await _newsService.GetAllAsync();
            var q = all.AsQueryable();
            if (from.HasValue) q = q.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue) q = q.Where(n => n.CreatedDate <= to.Value);

            var byCategory = q.GroupBy(n => n.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.ByCategory = byCategory;
            return View();
        }

        public async Task<IActionResult> ExportCsv(DateTime? from, DateTime? to)
        {
            var all = await _newsService.GetAllAsync();
            var q = all.AsQueryable();
            if (from.HasValue) q = q.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue) q = q.Where(n => n.CreatedDate <= to.Value);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("NewsArticleId,NewsTitle,CategoryId,CreatedDate,Status");
            foreach (var n in q.OrderByDescending(n => n.CreatedDate))
            {
                csv.AppendLine($"\"{n.NewsArticleId}\",\"{n.NewsTitle?.Replace('\"','\'')}\",{n.CategoryId},{(n.CreatedDate?.ToString("o") ?? "")},{(n.NewsStatus==true?1:0)}");
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "report.csv");
        }
    }
}

