using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories.Interfaces
{
    public interface IAccountRepository : IRepository<SystemAccount>
    {
        Task<SystemAccount?> GetByEmailAsync(string email);
        Task<SystemAccount?> LoginAsync(string email, string password);
        Task<bool> CanDeleteAsync(short accountId);
    }
}
