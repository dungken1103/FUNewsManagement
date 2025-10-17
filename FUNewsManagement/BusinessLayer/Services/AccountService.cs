using BusinessLayer.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        public AccountService(IAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<SystemAccount?> AuthenticateAsync(string email, string password)
        {
            return await _repo.LoginAsync(email, password);
        }

        public async Task<IEnumerable<SystemAccount>> SearchAsync(string? nameOrEmail, int? role)
        {
            var all = await _repo.GetAllAsync();
            var q = all.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nameOrEmail))
            {
                q = q.Where(a => (a.AccountName ?? string.Empty).Contains(nameOrEmail, StringComparison.OrdinalIgnoreCase)
                    || (a.AccountEmail ?? string.Empty).Contains(nameOrEmail, StringComparison.OrdinalIgnoreCase));
            }
            if (role.HasValue)
            {
                q = q.Where(a => a.AccountRole == role.Value);
            }
            return q.ToList();
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<SystemAccount?> GetByIdAsync(short id) => await _repo.GetByIdAsync(id);

        public async Task AddAsync(SystemAccount account)
        {
            await _repo.AddAsync(account);
            await _repo.SaveAsync();
        }

        public async Task UpdateAsync(SystemAccount account)
        {
            _repo.Update(account);
            await _repo.SaveAsync();
        }

        public async Task DeleteAsync(short id)
        {
            var acc = await _repo.GetByIdAsync(id);
            if (acc == null)
                throw new InvalidOperationException("Account not found.");

            if (!await _repo.CanDeleteAsync(id))
                throw new InvalidOperationException("Cannot delete account that has created articles.");

            _repo.Delete(acc);
            await _repo.SaveAsync();
        }

        public async Task ChangePasswordAsync(short accountId, string currentPassword, string newPassword)
        {
            var acc = await _repo.GetByIdAsync(accountId);
            if (acc == null) throw new InvalidOperationException("Account not found.");
            if (acc.AccountPassword != currentPassword) throw new InvalidOperationException("Current password is incorrect.");
            acc.AccountPassword = newPassword;
            _repo.Update(acc);
            await _repo.SaveAsync();
        }

        public async Task<bool> EmailExistsAsync(string email, short? excludeId = null)
        {
            var e = await _repo.GetByEmailAsync(email);
            if (e == null) return false;
            if (excludeId.HasValue && e.AccountId == excludeId.Value) return false;
            return true;
        }
    }
}
