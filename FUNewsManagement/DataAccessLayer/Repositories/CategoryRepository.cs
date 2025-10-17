using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(FUNewsManagementContext context) : base(context) { }
        public async Task<bool> CanDeleteAsync(short categoryId)
        {
            return !await _context.NewsArticles.AnyAsync(n => n.CategoryId == categoryId);
        }

    }
}
