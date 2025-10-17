using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class NewsService : INewsService
    {
        private readonly FUNewsManagementContext _context;
        private readonly INewsRepository _repo;
        public NewsService(INewsRepository repo, FUNewsManagementContext context) 
        { 
            _repo = repo;
            _context = context;
        }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task SaveAsync() => await _repo.SaveAsync();
        //public async Task RemoveAllTagsAsync(string newsArticleId)
        //{
        //    var news = await _repo.GetByIdAsync(newsArticleId);

        //    if (news != null)
        //    {
        //        // Load navigation property nếu chưa load
        //        await _repo.LoadTagsAsync(news);

        //        news.Tags.Clear(); // Xóa liên kết
        //        await _repo.SaveChangesAsync(); // Lưu thay đổi
        //    }
        //}


        public async Task<NewsArticle?> GetByIdAsync(string id) => await _repo.GetByIdAsync(id);

        public async Task<IEnumerable<NewsArticle>> SearchAsync(string? q, short? categoryId, DateTime? from, DateTime? to)
        {
            var all = await _repo.GetAllAsync();
            var ql = all.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) ql = ql.Where(n => (n.NewsTitle ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
            if (categoryId.HasValue) ql = ql.Where(n => n.CategoryId == categoryId.Value);
            if (from.HasValue) ql = ql.Where(n => n.CreatedDate >= from.Value);
            if (to.HasValue) ql = ql.Where(n => n.CreatedDate <= to.Value);
            return ql.OrderByDescending(n => n.CreatedDate).ToList();
        }

        public async Task CreateAsync(NewsArticle article, short currentUserId)
        {
            var all = await _repo.GetAllAsync();
            // determine next numeric id by scanning existing IDs and taking max of numeric ones
            long maxId = 0;
            foreach (var a in all)
            {
                if (!string.IsNullOrEmpty(a.NewsArticleId) && long.TryParse(a.NewsArticleId, out var parsed))
                {
                    if (parsed > maxId) maxId = parsed;
                }
            }
            var nextId = maxId + 1;
            article.NewsArticleId = nextId.ToString();

            article.CreatedById = currentUserId;
            article.CreatedDate = DateTime.Now;
            article.ModifiedDate = DateTime.Now;
            article.NewsStatus = true;

            await _repo.AddAsync(article);
            await _repo.SaveAsync();
        }




        public async Task AddAsync(NewsArticle news)
        {
            await _repo.AddAsync(news);
            await _repo.SaveAsync();
        }

        public async Task UpdateAsync(NewsArticle news)
        {
            // reconcile tags: load existing article including tags, then update the tag collection
            var existing = await _repo.GetByIdAsync(news.NewsArticleId);
            if (existing == null) throw new InvalidOperationException("Article not found");

            // update scalar properties
            existing.NewsTitle = news.NewsTitle;
            existing.Headline = news.Headline;
            existing.NewsContent = news.NewsContent;
            existing.NewsSource = news.NewsSource;
            existing.CategoryId = news.CategoryId;
            existing.NewsStatus = news.NewsStatus;
            existing.UpdatedById = news.UpdatedById;
            existing.ModifiedDate = news.ModifiedDate;

            // reconcile tags by TagId
            existing.Tags ??= new List<Tag>();
            var selectedTagIds = news.Tags?.Select(t => t.TagId).ToHashSet() ?? new HashSet<int>();

            // remove tags not selected
            var toRemove = existing.Tags.Where(t => !selectedTagIds.Contains(t.TagId)).ToList();
            foreach (var r in toRemove) existing.Tags.Remove(r);

            // add new tags
            var existingIds = existing.Tags.Select(t => t.TagId).ToHashSet();
            foreach (var id in selectedTagIds)
            {
                if (!existingIds.Contains(id)) existing.Tags.Add(new Tag { TagId = id });
            }

            _repo.Update(existing);
            await _repo.SaveAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var news = await _context.NewsArticles
                .Include(n => n.Tags) // đảm bảo load navigation
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);

            if (news == null)
                return;

            // Xóa bản ghi trong bảng trung gian NewsTag bằng cách xoá liên kết trong navigation collection
            if (news.Tags != null && news.Tags.Any())
            {
                news.Tags.Clear();
                // update the entity so EF Core is aware of the change to the navigation
                _context.NewsArticles.Update(news);
            }

            // Xóa bài viết chính
            _context.NewsArticles.Remove(news);

            await _context.SaveChangesAsync();
        }

        public async Task<NewsArticle> DuplicateAsync(string id, short currentUserId)
        {
            var original = await _repo.GetByIdAsync(id);
            if (original == null) throw new InvalidOperationException("Original article not found");

            // compute next numeric id similar to CreateAsync
            var all = await _repo.GetAllAsync();
            long maxId = 0;
            foreach (var a in all)
            {
                if (!string.IsNullOrEmpty(a.NewsArticleId) && long.TryParse(a.NewsArticleId, out var parsed))
                {
                    if (parsed > maxId) maxId = parsed;
                }
            }
            var nextId = (maxId + 1).ToString();

            var copy = new NewsArticle
            {
                NewsArticleId = nextId,
                NewsTitle = original.NewsTitle,
                Headline = original.Headline,
                NewsContent = original.NewsContent,
                NewsSource = original.NewsSource,
                CategoryId = original.CategoryId,
                NewsStatus = original.NewsStatus,
                CreatedById = currentUserId,
                CreatedDate = DateTime.Now,
                Tags = original.Tags != null ? new List<Tag>(original.Tags) : new List<Tag>()
            };

            await _repo.AddAsync(copy);
            await _repo.SaveAsync();
            return copy;
        }

        public async Task<IEnumerable<NewsArticle>> GetRelatedAsync(string id, short categoryId)
        {
            var list = await _repo.GetRelatedAsync(id, categoryId);
            return list.AsEnumerable();
        }
    }
}