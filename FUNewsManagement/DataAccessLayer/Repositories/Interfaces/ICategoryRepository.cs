using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> CanDeleteAsync(short categoryId);
    }
}
