// TagRepository.cs
using DataAccessLayer.Data;
using DataAccessLayer.Models;

namespace DataAccessLayer.Repositories
{
    public class TagRepository : Repository<Tag>, Interfaces.ITagRepository
    {
        public TagRepository(FUNewsManagementContext context) : base(context) { }
    }
}
