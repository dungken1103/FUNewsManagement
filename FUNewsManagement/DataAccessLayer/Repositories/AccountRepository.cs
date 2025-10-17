using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class AccountRepository : Repository<SystemAccount>, IAccountRepository
    {

        public AccountRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<bool> CanDeleteAsync(short accountId)
        {
            return !await _context.NewsArticles.AnyAsync(n => n.CreatedById == accountId);
        }


        public async Task<SystemAccount?> GetByEmailAsync(string email)
        {
            return await _context.SystemAccounts.FirstOrDefaultAsync(a => a.AccountEmail == email);
        }
        public async Task<SystemAccount?> LoginAsync(string email, string password)
        {
            return await _context.SystemAccounts
                .FirstOrDefaultAsync(a => a.AccountEmail == email && a.AccountPassword == password);
        }

    }
}
