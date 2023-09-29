
using DMU_Git.Data;
using DMU_Git.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DMU_Git.Services
{
    public class DynamicDbService
    {
        private readonly ApplicationDbContext _dbContext;
          
        public DynamicDbService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> CreateDynamicTableAsync(TableCreationRequest request)
        {
            try
            {
                // Create the table metadata
                var entityList = await CreateTableMetadataAsync(request);

                if (entityList == null)
                {
                    return false;
                }

                // Bind column metadata to the table
                await BindColumnMetadataAsync(request, entityList);

                // Create the SQL table
                var createTableSql = GenerateCreateTableSql(request);
                await _dbContext.Database.ExecuteSqlRawAsync(createTableSql);

                Console.WriteLine($"Table '{request.TableName}' created successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
                return false;
            }
        }

        private async Task<EntityListMetadataModel> CreateTableMetadataAsync(TableCreationRequest request)
        {
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
                Console.WriteLine($"Error creating table '{request.TableName}': {ex.Message}");
                return null;
            }
        }

        private async Task BindColumnMetadataAsync(TableCreationRequest request, EntityListMetadataModel entityList)
        {
            foreach (var column in request.Columns)
            {
                var entityColumn = new EntityColumnListMetadataModel
                {
                    EntityColumnName = column.EntityColumnName,
                    Datatype = column.DataType,
                    Length = column.Length,
                    IsNullable = column.IsNullable,
                    DefaultValue = column.DefaultValue,
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
                Console.WriteLine($"Error binding column metadata for table '{request.TableName}': {ex.Message}");
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

                // Handle different data types
                switch (column.DataType.ToLower()) // Convert to lowercase to handle case-insensitivity
                {
                    case "int":
                        createTableSql += "integer";
                        if (column.Length > 0)
                        {
                            createTableSql += $"({column.Length})";
                        }
                        break;
                    case "string":
                        createTableSql += $"varchar";
                        if (column.Length > 0)
                        {
                            createTableSql += $"({column.Length})";
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
                    case "Date":
                        createTableSql += "date";
                        break;
                    case "time":
                        createTableSql += "time";
                        break;
                    case "timestamp":
                        createTableSql += "timestamp";
                        break;
                    // Add more data type cases as needed
                    default:
                        // Handle unsupported data types or provide a default
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

            createTableSql += ");";
            return createTableSql;
        }
    }
}

