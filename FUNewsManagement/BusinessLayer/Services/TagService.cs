using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services
{
    public class TagService : ITagService
    {
        private readonly FUNewsManagementContext _context;
        private readonly ITagRepository _repo;
    public TagService(ITagRepository repo,FUNewsManagementContext context) { _repo = repo; _context = context; }

        public async Task<IEnumerable<Tag>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<Tag?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task AddAsync(Tag tag)
        {
            var all = await _repo.GetAllAsync();
            int newId = (all.Any() ? all.Max(t => t.TagId) : 0) + 1;
            tag.TagId = newId;

            await _repo.AddAsync(tag);
            await _repo.SaveAsync();
        }


        public async Task UpdateAsync(Tag tag)
        {
            var existing = await _repo.GetByIdAsync(tag.TagId);
            if (existing == null) return;

            existing.TagName = tag.TagName;
            existing.Note = tag.Note;

            _repo.Update(existing);
            await _repo.SaveAsync();
        }


        // public async Task DeleteAsync(int id)
        // {
        //     var t = await _repo.GetByIdAsync(id);
        //     if (t != null)
        //     {
        //         _repo.Delete(t);
        //         await _repo.SaveAsync();
        //     }
        // }
        public async Task DeleteAsync(int id)
        {
            // Tên biến và kiểu dữ liệu phù hợp cho Tag
            var tag = await _context.Tags
                .Include(t => t.NewsArticles) // đảm bảo load navigation many-to-many
                .FirstOrDefaultAsync(t => t.TagId == id);

            if (tag == null)
                return;

            // Xóa liên kết trong bảng trung gian NewsTag bằng cách clear navigation collection
            if (tag.NewsArticles != null && tag.NewsArticles.Any())
            {
                tag.NewsArticles.Clear();
                _context.Tags.Update(tag);
                await _context.SaveChangesAsync(); // lưu trước để xóa các bản ghi trung gian
            }

            // Xóa thẻ
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }
}