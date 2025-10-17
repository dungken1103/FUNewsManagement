using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class NewsRepository : Repository<NewsArticle>, INewsRepository
    {
        public NewsRepository(FUNewsManagementContext context) : base(context) { }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync()
        {
            return await _context.NewsArticles
                                 .Include(n => n.Category)
                                 .Include(n => n.CreatedBy) 
                                 .Include(n => n.Tags)
                                 .ToListAsync();
        }
        public async Task<NewsArticle?> GetByIdAsync(string id)
        {
            return await _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy) 
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        }

        public async Task<List<NewsArticle>> GetByAuthorAsync(short authorId)
        {
            return await _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .Include(n => n.Tags)
                .Where(n => n.CreatedById == authorId)
                .ToListAsync();
        }
        public async Task LoadTagsAsync(NewsArticle news)
        {
            await _context.Entry(news).Collection(n => n.Tags).LoadAsync();
        }

        public async Task<List<NewsArticle>> GetRelatedAsync(string currentNewsId, short categoryId)
        {
            var relatedByCategory = await _context.NewsArticles
                .Where(n => n.CategoryId == categoryId && n.NewsArticleId != currentNewsId && n.NewsStatus == true)
                .Take(3)
                .ToListAsync();

            if (relatedByCategory.Count < 3)
            {
                // find tag ids for the current news article via navigation property
                var currentTags = await _context.NewsArticles
                    .Where(n => n.NewsArticleId == currentNewsId)
                    .SelectMany(n => n.Tags.Select(t => t.TagId))
                    .ToListAsync();

                if (currentTags.Any())
                {
                    var relatedByTag = await _context.NewsArticles
                        .Where(n => n.NewsArticleId != currentNewsId && n.NewsStatus == true && n.Tags.Any(t => currentTags.Contains(t.TagId)))
                        .Distinct()
                        .Take(3 - relatedByCategory.Count)
                        .ToListAsync();

                    relatedByCategory.AddRange(relatedByTag);
                }
            }

            return relatedByCategory;
        }

        public async Task DeleteNewsTagsByArticleIdAsync(string newsArticleId)
        {
            // remove rows from the join table NewsTag where NewsArticleId = newsArticleId
            var rows = _context.Set<Dictionary<string, object>>()
                .FromSqlRaw("SELECT * FROM NewsTag WHERE NewsArticleID = {0}", newsArticleId)
                .ToList();

            if (rows.Any())
            {
                // delete by executing a raw SQL command
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM NewsTag WHERE NewsArticleID = {0}", newsArticleId);
            }
        }


    }
}
