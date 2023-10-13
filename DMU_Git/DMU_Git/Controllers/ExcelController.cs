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
using DMU_Git.Services;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMU_Git.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularDev")]
    public class ExcelController : Controller
    {

        private readonly IExcelService _excelService;
        protected APIResponse _response;
        public ExcelController(IExcelService excelService)
        {

            _excelService = excelService;
            _response = new();
        }

        [HttpPost("generate")]
        public IActionResult GenerateExcelFile([FromBody] List<EntityColumnDTO> columns)
        {
            try
            {
                // Convert column names to lowercase
                //var lowercaseColumns = columns.Select(col => new EntityColumnDTO { EntityColumnName = col.EntityColumnName.ToLower() }).ToList();
                byte[] excelBytes = _excelService.GenerateExcelFile(columns);

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
        public async Task<IActionResult> UploadFile(IFormFile file, string tableName)
        {
            var mytablername = tableName;

            if (file == null || file.Length == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("No file uploaded.");
                return BadRequest(_response);
            }

            string fileName = file.FileName;

            if (string.IsNullOrEmpty(tableName))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("Table name is required.");
                return BadRequest(_response);
            }

            try
            {
                // Check if the table exists in the database
                //if (!TableExists(mytablername))
                //{
                //    _response.StatusCode = HttpStatusCode.NotFound;
                //    _response.IsSuccess = false;
                //    _response.ErrorMessage.Add($"Table '{mytablername}' does not exist in the database.");
                //    return NotFound(_response);
                //}
                var columnsDTO = _excelService.GetColumnsForEntity(tableName).ToList();
                var excelData = _excelService.ReadExcelFromFormFile(file);
                DataTable validRowsDataTable = excelData.Clone(); // Create a DataTable to store valid rows
                DataTable successdata = validRowsDataTable.Clone(); // Create a DataTable to store valid rows
                List<string> badRows = new List<string>();
                List<string> columns = new List<string>();
                using (var excelFileStream = file.OpenReadStream())
                {
                    var data = _excelService.ReadDataFromExcel(excelFileStream,excelData.Rows.Count);

                    if (data == null || data.Count == 0)
                    {
                        _response.StatusCode = HttpStatusCode.NoContent;
                        _response.ErrorMessage.Add($"No data found in the '{mytablername}' template");
                        _response.IsSuccess = false;
                        return Ok(_response);
                    }

                    // Based NotNull Field
                    foreach (var row in data)
                    {
                        foreach (var col in row.Keys)
                        {
                            var value = row[col];
                            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                            {
                                _response.StatusCode = HttpStatusCode.BadRequest;
                                _response.IsSuccess = false;
                                _response.ErrorMessage.Add("Empty or null value found in column '{col}'");

                                return BadRequest(_response);
                            }
                        }
                    }
                    var errorcount = 0;
                    var successcount = 0;

                    // Get the columns from the first row (assuming all rows have the same structure)
                    var columnnames = data.First().Keys.ToList();

                    columns = columnnames.ToList();

                    //Data Type Validation
                    for (int row = 0; row < excelData.Rows.Count; row++)
                    {
                        bool rowValidationFailed = false; // Flag to track row validation

                        for (int col = 0; col < excelData.Columns.Count; col++)
                        {
                            string cellData = excelData.Rows[row][col].ToString();
                            EntityColumnDTO columnDTO = columnsDTO[col];
                            if (!_excelService.IsValidDataType(cellData, columnDTO.Datatype))
                            {
                                // Set the flag to indicate validation failure for this row
                                rowValidationFailed = true;
                                break; // Exit the loop as soon as a validation failure is encountered
                            }
                        }
                        // If row validation succeeded, add the entire row to the validRowsDataTable
                        if (!rowValidationFailed)
                        {
                            validRowsDataTable.Rows.Add(excelData.Rows[row].ItemArray);
                        }

                        // If row validation failed, add the entire row data as a comma-separated string to the badRows list
                        if (rowValidationFailed)
                        {
                            string badRow = string.Join(",", excelData.Rows[row].ItemArray); // Join the row data with commas
                            badRows.Add(badRow);
                        }
                    }
                    //Primary Key Validation
                    List<int> primaryKeyColumns = new List<int>();
                    for (int col = 0; col < validRowsDataTable.Columns.Count; col++)
                    {
                        EntityColumnDTO columnDTO = columnsDTO[col];
                        if (columnDTO.ColumnPrimaryKey)
                        {
                            primaryKeyColumns.Add(col);
                        }
                    }
                    HashSet<string> seenValues = new HashSet<string>(); // To store values in primary key columns for duplicate checking
                    for (int row = 0; row < validRowsDataTable.Rows.Count; row++)
                    {
                        bool rowValidationFailed = false; // Flag to track row validation
                        foreach (var col in primaryKeyColumns)
                        {
                            string cellData = validRowsDataTable.Rows[row][col].ToString();
                            if (string.IsNullOrWhiteSpace(cellData) || seenValues.Contains(cellData))
                            {
                                // Set the flag to indicate validation failure for this row
                                rowValidationFailed = true;
                                break; // Exit the loop as soon as a validation failure is encountered
                            }
                            seenValues.Add(cellData);
                        }


                        if (!rowValidationFailed)
                        {
                            successdata.Rows.Add(validRowsDataTable.Rows[row].ItemArray);
                        }
                        // If row validation failed, add the entire row data as a comma-separated string to the badRows list
                        if (rowValidationFailed)
                        {
                            string badRow = string.Join(",", validRowsDataTable.Rows[row].ItemArray); // Join the row data with commas
                            badRows.Add(badRow);
                        }
                    }
                }
                /////store log data
                var result = await _excelService.Createlog(tableName, badRows,fileName, successdata);

                // Build the values for the SQL INSERT statement
                _excelService.InsertDataFromDataTableToPostgreSQL(successdata, tableName, columns);

                _response.Result = result;
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.ErrorMessage.Add("Data saved to the database Successfully.");
                return Ok(_response);

            }
            catch (Exception ex)
            {

                string[] errorParts = ex.Message.Split(':');
                if (errorParts.Length >= 2)
                {
                    string[] errorMessageParts = errorParts[1].Split('\n');
                    string errorMessage = errorMessageParts[0].Trim();
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false
                    };
                    response.ErrorMessage.Add(errorMessage);
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }
                else
                {
                    // Handle cases where the error message may not be in the expected format
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false
                    };
                    response.ErrorMessage.Add(ex.Message);
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }
            }
        }

        //private bool TableExists(string tableName)
        //{
        //    try
        //    {
        //        using (var connection =  _context.Database.GetDbConnection())
        //        {
        //            connection.Open();

        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = @TableName";
        //                command.Parameters.Add(new NpgsqlParameter("TableName", NpgsqlDbType.Text) { Value = tableName });

        //                var result = command.ExecuteScalar();

        //                if (result != null && result != DBNull.Value)
        //                {
        //                    return Convert.ToInt32(result) > 0;
        //                }
        //            }

        //            connection.Close();
        //        }

        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}


    }
}



