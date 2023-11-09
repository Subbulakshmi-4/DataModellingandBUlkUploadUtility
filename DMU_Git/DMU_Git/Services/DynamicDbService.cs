using DMU_Git.Data;
using DMU_Git.Models;
using Microsoft.EntityFrameworkCore;


namespace DMU_Git.Services
{
    public class DynamicDbService
    {
        private readonly ApplicationDbContext _dbContext;
        public DynamicDbService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> TableExistsAsync(string tableName)
        {
            var lowerCaseTableName = tableName.ToLower();
            var existingEntity = await _dbContext.EntityListMetadataModels
                .AnyAsync(e => e.EntityName.ToLower() == lowerCaseTableName);

            return existingEntity;
        }

        public async Task<bool> CreateDynamicTableAsync(TableCreationRequest request)
        {
            try
            {
                var entityList = await CreateTableMetadataAsync(request);
                if (entityList == null)
                {
                    return false;
                }
                await BindColumnMetadataAsync(request, entityList);
                var createTableSql = GenerateCreateTableSql(request);
                await _dbContext.Database.ExecuteSqlRawAsync(createTableSql);
                return true;
            }
            catch (Exception ex)
            {
             
                return false;
            }
        }

        private async Task<EntityListMetadataModel> CreateTableMetadataAsync(TableCreationRequest request)
        {
            var lowerCaseTableName = request.TableName.ToLower();
            var existingEntity = await _dbContext.EntityListMetadataModels
                .FirstOrDefaultAsync(e => e.EntityName.ToLower() == lowerCaseTableName);
            if (existingEntity != null)
            {
                return existingEntity;
            }
            var entityList = new EntityListMetadataModel
            {
                EntityName = request.TableName,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
            };
            _dbContext.EntityListMetadataModels.Add(entityList);
            try
            {
                await _dbContext.SaveChangesAsync();
                return entityList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task BindColumnMetadataAsync(TableCreationRequest request, EntityListMetadataModel entityList)
        {
            foreach (var column in request.Columns)
            {
                var existingColumn = await _dbContext.EntityColumnListMetadataModels
                    .FirstOrDefaultAsync(c => c.EntityColumnName.ToLower() == column.EntityColumnName.ToLower() && c.EntityId == entityList.Id);

                if (existingColumn != null)
                {
                    continue;
                }
                var entityColumn = new EntityColumnListMetadataModel
                {
                    EntityColumnName = column.EntityColumnName,
                    Datatype = column.DataType,
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
                    ListEntityId = column.ListEntityId,
                    ListEntityKey = column.ListEntityKey,
                    ListEntityValue = column.ListEntityValue,
                    True = column.True,
                    False = column.False,
                    ColumnPrimaryKey = column.ColumnPrimaryKey,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    EntityId = entityList.Id
                };

                _dbContext.EntityColumnListMetadataModels.Add(entityColumn);
            }
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
              
            }
        }
       

        private string GenerateCreateTableSql(TableCreationRequest request)
        {
            var createTableSql = $"CREATE TABLE \"{request.TableName}\" (";
            bool hasColumns = false;
            foreach (var column in request.Columns)
            {
                if (hasColumns)
                {
                    createTableSql += ",";
                }
                createTableSql += $"\"{column.EntityColumnName}\" ";
                switch (column.DataType.ToLower())
                {
                    case "int":
                        createTableSql += "integer";
                        break;
                    case "date":
                        createTableSql += "date";
                        break;
                    case "string":
                        createTableSql += $"varchar";
                        if (column.MaxLength > 0)
                        {
                            createTableSql += $"({column.MaxLength})";
                        }
                        else
                        {
                            createTableSql += "(255)";
                        }
                        break;
                    case "char":
                        createTableSql += $"char";
                        if (column.Length == 1)
                        {
                            createTableSql += $"({column.Length})";
                        }
                        break;
                    case "boolean":
                        createTableSql += "boolean";
                        break;
                    case "time":
                        createTableSql += "time";
                        break;
                    case "timestamp":
                        createTableSql += "timestamp";
                        break;
                    default:
                        createTableSql += "varchar";
                        break;
                }
                if (!column.IsNullable)
                {
                    createTableSql += " NOT NULL";
                }
                if (!string.IsNullOrEmpty(column.DefaultValue))
                {
                    createTableSql += $" DEFAULT '{column.DefaultValue}'";
                }
                if (column.ColumnPrimaryKey)
                {
                    createTableSql += " PRIMARY KEY";
                }
                hasColumns = true;
            }
            createTableSql += hasColumns ? "," : "";
            createTableSql += "\"createddate\" timestamp DEFAULT CURRENT_TIMESTAMP";
            createTableSql += ");";
            return createTableSql;
        }
    }
}

