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
                var lowerCaseTableName = request.TableName.ToLower();

    // Check if table with the same name already exists
    var existingEntity = await _dbContext.EntityListMetadataModels
        .FirstOrDefaultAsync(e => e.EntityName.ToLower() == lowerCaseTableName);



            if (existingEntity != null)
            {
                Console.WriteLine($"Table '{request.TableName}' already exists.");
                return existingEntity;
            }

            // Create the table metadata if it doesn't exist
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
                // Check if a column with the same name already exists
                var existingColumn = await _dbContext.EntityColumnListMetadataModels
                    .FirstOrDefaultAsync(c => c.EntityColumnName.ToLower() == column.EntityColumnName.ToLower() && c.EntityId == entityList.Id);

                if (existingColumn != null)
                {
                    // Handle the situation where the column already exists.
                    // You can update the existing column or log an error, depending on your use case.
                    Console.WriteLine($"Column '{column.EntityColumnName}' already exists in table '{request.TableName}'.");
                    continue; // Skip adding this column and move to the next one.
                }

                var entityColumn = new EntityColumnListMetadataModel
                {
                    EntityColumnName = column.EntityColumnName,
                    Datatype = column.DataType,
                    Length = column.Length,
                   StringMinLength = column.StringMinLength,
                   StringMaxLength = column.StringMaxLength,
                   NumberMinValue = column.NumberMinValue,
                   NumberMaxValue = column.NumberMaxValue,
                   DateMinValue = column.DateMinValue,
                    DateMaxValue = column.DateMaxValue,
                    Description = column.Description,
                    IsNullable = column.IsNullable,
                    DefaultValue = column.DefaultValue,
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
                Console.WriteLine($"Error binding column metadata for table '{request.TableName}': {ex.Message}");
                // Handle the exception as appropriate for your application.
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

                        // Add minimum value constraint for int columns
                        if (column.NumberMinValue.HasValue)
                        {
                            createTableSql += $" CHECK (\"{column.EntityColumnName}\" >= {column.NumberMinValue})";
                        }

                        // Add maximum value constraint for int columns
                        if (column.NumberMaxValue.HasValue)
                        {
                            createTableSql += $" CHECK (\"{column.EntityColumnName}\" <= {column.NumberMaxValue})";
                        }
                        break;
                    case "date":
                        createTableSql += "date";

                        // Add minimum date constraint for date columns
                        if (column.DateMinValue.HasValue)
                        {
                            createTableSql += $" CHECK (\"{column.EntityColumnName}\" >= '{column.DefaultValue:yyyy-MM-dd}')";
                        }

                        // Add maximum date constraint for date columns
                        if (column.DateMaxValue.HasValue)
                        {
                            createTableSql += $" CHECK (\"{column.EntityColumnName}\" <= '{column.DateMaxValue:yyyy-MM-dd}')";
                        }
                        break;

                    case "string":
                        createTableSql += $"varchar";
                        if (column.StringMaxLength > 0)
                        {
                            createTableSql += $"({column.StringMaxLength})";
                        }
                        else
                        {
                            // Set default length if not specified
                            createTableSql += "(255)";
                        }

                        // Add minimum length constraint
                        if (column.StringMinLength > 0)
                        {
                            createTableSql += $" CHECK (LENGTH(\"{column.EntityColumnName}\") >= {column.StringMinLength})";
                        }

                        // Add maximum length constraint
                        if (column.StringMaxLength > 0)
                        {
                            createTableSql += $" CHECK (LENGTH(\"{column.EntityColumnName}\") <= {column.StringMaxLength})";
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

