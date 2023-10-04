using DMU_Git.Models;
using DMU_Git.Models.DTO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using DMU_Git.Data;
using DMU_Git.Services.Interface;
using Npgsql;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace DMU_Git.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularDev")]
    public class ExcelController : Controller
    {
        private readonly IExcelService _excelService;
        private readonly ApplicationDbContext _context;
        protected APIResponse _response;


        public ExcelController(IExcelService excelService, ApplicationDbContext context)
        {

            _excelService = excelService;
            _context = context;
            _response = new();
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));

        }

        [HttpPost("generate")]
        public IActionResult GenerateExcelFile([FromBody] List<EntityColumnDTO> columns)
        {
            try
            {
                // Convert column names to lowercase
                var lowercaseColumns = columns.Select(col => new EntityColumnDTO { EntityColumnName = col.EntityColumnName.ToLower() }).ToList();
                byte[] excelBytes = _excelService.GenerateExcelFile(lowercaseColumns);

                // Create a response for downloading the Excel file
                var fileContentResult = new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "columns.xlsx"
                };

                return fileContentResult;
            }
            catch (Exception ex)
            {

                var apiResponse = new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessage = new List<string> { ex.Message },
                    Result = null
                };

                return StatusCode((int)HttpStatusCode.InternalServerError, apiResponse);
            }
        }



        [HttpPost("upload")]
        public IActionResult UploadFile(IFormFile file, string tableName)
        {
            //var mydatabasename = databaseName;
            var mytablername = tableName;
            if (file == null || file.Length == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess=false;
                _response.ErrorMessage.Add("No file uploaded.");
                return BadRequest(_response);
            }

            try
            {
                using (var excelFileStream = file.OpenReadStream())
                {
                    var data = _excelService.ReadDataFromExcel(excelFileStream);


                    if (data == null || data.Count == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage.Add($"No data found in the '{mytablername}' template");
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }
                    // Check for empty or null values in the data
                    foreach (var row in data)
                    {
                        foreach (var col in row.Keys)
                        {
                            var value = row[col];
                            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                _response.StatusCode = HttpStatusCode.BadRequest;
                                _response.ErrorMessage.Add($"Empty or null value found in column '{col}'");
                                _response.IsSuccess = false;
                                return BadRequest(_response);
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(tableName))
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage.Add("Table name is required.");
                        _response.IsSuccess = false;
                        return BadRequest(_response);

                    }
                    // Check if the table exists in the database
                    if (!TableExists(mytablername))
                    {
                        _response.StatusCode = HttpStatusCode.NotFound;
                        _response.ErrorMessage.Add($"Table '{mytablername}' does not exist in the database.");
                        _response.IsSuccess = false;
                        return NotFound(_response);
                    }
               

                    // Get the columns from the first row (assuming all rows have the same structure)
                    var columns = data.First().Keys.ToList();
             
                    // Build the values for the SQL INSERT statement
                    var values = data.Select(row =>
                        $"({string.Join(", ", columns.Select(col => $"'{row[col]}'"))})");

                    var insertQuery = $"INSERT INTO public.\"{mytablername}\" ({string.Join(", ", columns.Select(col => $"\"{col}\""))}) VALUES {string.Join(", ", values)}";
                    
                    string connectionString = $"Host=localhost;Database=CheckingTable;Username=postgres;Password=pos@sql";
                    //string connectionString = $"Host=localhost;Database={dbName};Username=postgres;Password=pos@sql";

                    using (var connection = new NpgsqlConnection(connectionString)) // Replace with connection string
                    {
                        connection.Open();

                        using (var command = new NpgsqlCommand(insertQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    _response.StatusCode = HttpStatusCode.Created;
                    _response.IsSuccess = true;
                    _response.ErrorMessage.Add("Data saved to the database.");
                    return Ok(_response);

                }
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add(ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
                //return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private bool TableExists(string tableName)
        {
            try
            {
                using (var connection = _context.Database.GetDbConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = @TableName";
                        command.Parameters.Add(new NpgsqlParameter("TableName", NpgsqlDbType.Text) { Value = tableName });

                        var result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result) > 0;
                        }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

       
       
        

    }
    }



