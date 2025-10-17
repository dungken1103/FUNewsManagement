// INewsService.cs
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<NewsArticle>> GetAllAsync();
        Task<NewsArticle?> GetByIdAsync(string id);
        Task<IEnumerable<NewsArticle>> SearchAsync(string? q, short? categoryId, DateTime? from, DateTime? to);
        Task<IEnumerable<NewsArticle>> GetRelatedAsync(string id, short categoryId);
        Task CreateAsync(NewsArticle article, short currentUserId);
        Task AddAsync(NewsArticle news);
        Task UpdateAsync(NewsArticle news);
        Task DeleteAsync(string id);
        Task SaveAsync();
        //Task RemoveAllTagsAsync(string newsArticleId);

        Task<NewsArticle> DuplicateAsync(string id, short currentUserId);
    }
}