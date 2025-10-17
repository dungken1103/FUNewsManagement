// ICategoryService.cs
using DataAccessLayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> SearchAsync(string? q);
        Task<Category?> GetByIdAsync(short id);
        Task AddAsync(Category cat);
        Task UpdateAsync(Category cat);
        Task DeleteAsync(short id);
        Task<IEnumerable<object>> GetCountsPerCategoryAsync();
    }
}