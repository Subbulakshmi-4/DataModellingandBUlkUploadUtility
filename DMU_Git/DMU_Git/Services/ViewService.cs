using DMU_Git.Data;
using DMU_Git.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DMU_Git.Services
{
    public class ViewService
    {
        private readonly ApplicationDbContext _context;

        public ViewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<EntityColumnListMetadataModel> GetColumnsForEntity(string entityName)
        {
            var entity = _context.EntityListMetadataModels.Include(e => e.EntityColumnListMetadata) // Include related columns
                .FirstOrDefault(e => e.EntityName == entityName);

            return entity?.EntityColumnListMetadata;
        }
    }
}
