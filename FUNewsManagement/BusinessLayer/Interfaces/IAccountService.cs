using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Models;

namespace BusinessLayer.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<SystemAccount>> GetAllAsync();
        Task<IEnumerable<SystemAccount>> SearchAsync(string? nameOrEmail, int? role);
        Task<SystemAccount?> GetByIdAsync(short id);
        Task AddAsync(SystemAccount account);
        Task UpdateAsync(SystemAccount account);
        Task DeleteAsync(short id);
        Task<SystemAccount?> AuthenticateAsync(string email, string password);
        Task ChangePasswordAsync(short accountId, string currentPassword, string newPassword);
        Task<bool> EmailExistsAsync(string email, short? excludeId = null);
    }
}
