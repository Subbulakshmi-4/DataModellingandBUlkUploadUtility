using DMU_Git.Data;
using DMU_Git.Models.DTO;

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
            var entity = _context.EntityListMetadataModels.FirstOrDefault(e => e.EntityName == entityName);

            if (entity == null)
            {
                // Entity not found, return a 404 Not Found response
                return null;
            }

            var columnsDTO = _context.EntityColumnListMetadataModels
                .Where(column => column.EntityId == entity.Id)
                .Select(column => new EntityColumnDTO
                {
                    Id = column.Id,
                    EntityId = entity.Id,
                    EntityColumnName = column.EntityColumnName,
                    Datatype = column.Datatype,
                    Length = column.Length,
                    MinLength = column.MinLength,
                    MaxLength = column.MaxLength,
                    MinRange = column.MinRange,
                    MaxRange = column.MaxRange,
                    DateMinValue = column.DateMinValue,
                    DateMaxValue = column.DateMaxValue,
                    Description = column.Description,
                    IsNullable = column.IsNullable,
                    DefaultValue = column.DefaultValue,
                    ColumnPrimaryKey = column.ColumnPrimaryKey,
                    True = column.True,
                    False = column.False
                }).ToList();

            if (columnsDTO.Count == 0)
            {
                // No columns found, return a 404 Not Found response with an error message
                return null;
            }

            return columnsDTO;
        }


    }
}
