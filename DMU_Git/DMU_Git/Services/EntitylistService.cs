using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;

namespace DMU_Git.Services
{
    public class EntitylistService : IEntitylistService
    {
        private readonly ApplicationDbContext _context;

        public EntitylistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<EntityListDto> GetEntityList()
        {
            return _context.EntityListMetadataModels.Select(entlist => new EntityListDto { EntityName = entlist.EntityName }).ToList();
        }
            


    }
}
