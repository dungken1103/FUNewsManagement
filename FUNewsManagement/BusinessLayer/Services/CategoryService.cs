using BusinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CategoryService : ICategoryService
    {
    private readonly ICategoryRepository _repo;
    public CategoryService(ICategoryRepository repo) { _repo = repo; }

        public async Task<IEnumerable<Category>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<Category?> GetByIdAsync(short id) => await _repo.GetByIdAsync(id);

        public async Task AddAsync(Category cat)
        {
            await _repo.AddAsync(cat);
            await _repo.SaveAsync();
        }

        public async Task UpdateAsync(Category cat)
        {
            _repo.Update(cat);
            await _repo.SaveAsync();
        }

        public async Task DeleteAsync(short id)
        {
            if (!await _repo.CanDeleteAsync(id))
                throw new InvalidOperationException("Cannot delete category used in articles.");
            await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<Category>> SearchAsync(string? q)
        {
            var all = await _repo.GetAllAsync();
            if (string.IsNullOrWhiteSpace(q)) return all;
            return all.Where(c => c.CategoryName.Contains(q, StringComparison.OrdinalIgnoreCase)
                || c.CategoryDesciption.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<object>> GetCountsPerCategoryAsync()
        {
            // Use context via repository implementation - repository doesn't expose context so use GetAll and counts in memory
            var all = await _repo.GetAllAsync();
            var counts = all.Select(c => new { c.CategoryId, c.CategoryName, Count = c.NewsArticles?.Count ?? 0 });
            return counts.Cast<object>().ToList();
        }

    }
}