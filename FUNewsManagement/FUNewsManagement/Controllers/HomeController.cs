using System.Diagnostics;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Models;
using FUNewsManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace FUNewsManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        protected readonly INewsService _newsService;
        protected readonly ICategoryService _categoryService;
        protected readonly ITagService _tagService;
        public HomeController(ILogger<HomeController> logger, INewsService newsService, ICategoryService categoryService, ITagService tagService)
        {
            _logger = logger; 
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        public async Task<IActionResult> Index(string? q, short? categoryId, DateTime? from, DateTime? to)
        {
            var list = await _newsService.SearchAsync(q, categoryId, from, to);
            ViewBag.Categories = await _categoryService.GetAllAsync() ?? new List<DataAccessLayer.Models.Category>();
            ViewBag.Tags = await _tagService.GetAllAsync() ?? new List<DataAccessLayer.Models.Tag>();
            return View(list);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
