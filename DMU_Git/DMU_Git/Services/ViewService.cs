using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
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

        public IEnumerable<EntityColumnDTO> GetColumnsForEntity(string entityName)
        {
            var entity = _context.EntityListMetadataModels.Include(e => e.EntityColumnListMetadata) // Include related columns
                .FirstOrDefault(e => e.EntityName == entityName);

            if (entity == null)
            {
                return null;
            }

            // Manual mapping to DTO
            var columnsDTO = entity.EntityColumnListMetadata.Select(column => new EntityColumnDTO
            {
                Id = column.Id,
                EntityColumnName = column.EntityColumnName,
                Datatype = column.Datatype,
                Length = column.Length,
                IsNullable = column.IsNullable,
                DefaultValue = column.DefaultValue,
                ColumnPrimaryKey = column.ColumnPrimaryKey
            }).ToList();

            return columnsDTO;
        }
    }
}
