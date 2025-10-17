using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface INewsRepository : IRepository<NewsArticle>
    {
        Task<List<NewsArticle>> GetRelatedAsync(string currentNewsId, short categoryId);
    }
}
