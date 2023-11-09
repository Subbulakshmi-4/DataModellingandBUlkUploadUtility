

using DMU_Git.Models;
using DMU_Git.Models.DTO;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using DMU_Git.Data;
using DMU_Git.Services.Interface;
using System.Data;
using Aspose.Cells;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace DMU_Git.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAngularDev")]
    public class ExcelController : Controller
    {
        private readonly IExcelService _excelService;
        protected APIResponse _response;
        private readonly ApplicationDbContext _context;
        public ExcelController(IExcelService excelService, ApplicationDbContext context)
        {
            _excelService = excelService;
            _response = new();
            _context = context;
        }

        [HttpPost("generate")]
        public IActionResult GenerateExcelFile([FromBody] List<EntityColumnDTO> columns, int? parentId)
        {
            try
            {
                // Convert column names to lowercase
                //var lowercaseColumns = columns.Select(col => new EntityColumnDTO { EntityColumnName = col.EntityColumnName.ToLower() }).ToList();
                byte[] excelBytes = _excelService.GenerateExcelFile(columns, parentId);

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
            List<string> errorMessages = new List<string>();
            string successMessage = null;

            var columnsDTO = _excelService.GetColumnsForEntity(tableName).ToList();


            int? uploadedEntityId = null;
            uploadedEntityId = _excelService.GetEntityIdByEntityNamefromui(tableName);
            var idfromtemplatesheet1 = _excelService.GetEntityIdFromTemplate(file, 0); // Sheet 1
            var idfromtemplatesheet2 = _excelService.GetEntityIdFromTemplate(file, 1); // Sheet 2

            if (idfromtemplatesheet1 != uploadedEntityId && idfromtemplatesheet2 != uploadedEntityId)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("Uploaded excel file is not valid, use template file to upload the data");
                return BadRequest(_response);
            }

            var excelData = _excelService.ReadExcelFromFormFile(file);
            if (excelData == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("No valid data found in the uploaded file.");
                return BadRequest(_response);
            }
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

            bool checkingtableName = _excelService.TableExists(tableName);
            if (checkingtableName == false)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("Table does not exist");
                return BadRequest(_response);
            }
            DataTable validRowsDataTable = excelData.Clone(); // Create a DataTable to store valid rows
            //Add the "RowNumber" column to the DataTable
            //validRowsDataTable.Columns.Add("RowNumber", typeof(int));
            DataTable successdata = validRowsDataTable.Clone(); // Create a DataTable to store valid rows
            List<string> badRows = new List<string>();
            List<string> filedatas = new List<string>();
            List<string> ErrorRowNumber = new List<string>();
            List<string> errorcolumnnames = new List<string>();
            List<string> columns = new List<string>();
            int total_count = excelData.Rows.Count;

            string comma_separated_string = null;
            try
            {
                string columnName = string.Empty;
                string primaryKey = null;
                foreach (var val in columnsDTO)
                {
                    if (val.ColumnPrimaryKey == true)
                    {
                        primaryKey = val.EntityColumnName;
                    }
                }
                using (var excelFileStream = file.OpenReadStream())
                {
                    var data = _excelService.ReadDataFromExcel(excelFileStream, excelData.Rows.Count);
                    if (data == null || data.Count == 0)
                    {
                        _response.StatusCode = HttpStatusCode.NoContent;
                        _response.ErrorMessage.Add($"No data found in the '{mytablername}' template");
                        _response.IsSuccess = false;
                        return Ok(_response);
                    }

                    // Get the columns from the first row
                    var columnnames = data.First().Keys.ToList();
                    columns = columnnames.ToList();
                    comma_separated_string = string.Join(",", columns.ToArray());


                    //NotNull Validation
                    for (int row = 0; row < excelData.Rows.Count; row++)
                    {
                        bool rowValidationFailed = false; // Flag to track row validation

                        string badRow = string.Join("!", excelData.Rows[row].ItemArray); // Join the row data with commas
                        for (int col = 0; col < excelData.Columns.Count - 1; col++)

                        {
                            string cellData = excelData.Rows[row][col].ToString();

                            EntityColumnDTO columnDTO = columnsDTO[col];

                            if (columnDTO.IsNullable == true && cellData == "")
                            {
                                rowValidationFailed = true;
                                badRows.Add(badRow); // Store the row data
                                if (!errorcolumnnames.Contains(columnDTO.EntityColumnName))
                                {
                                    errorcolumnnames.Add(columnDTO.EntityColumnName);
                                }

                                break;
                            }

                        }
                        // If row validation succeeded, add the entire row to the validRowsDataTable
                        if (!rowValidationFailed)
                        {
                            validRowsDataTable.Rows.Add(excelData.Rows[row].ItemArray);
                        }
                    }

                    if (badRows.Count > 0)
                    {
                        string values = string.Join(",", badRows.Select(row => row.Split(',').Last()));
                        ErrorRowNumber.Add(values);
                        badRows.Insert(0, comma_separated_string);
                        List<string> modifiedRows = badRows.Select(row => row.Substring(0, row.LastIndexOf(','))).ToList();
                        badRows = modifiedRows;
                        string delimiter = ";"; // Specify the delimiter you want
                        string delimiter1 = "!"; // Specify the delimiter you want
                        string baddatas = string.Join(delimiter, badRows);
                        string badcolumns = string.Join(delimiter1, errorcolumnnames);
                        filedatas.Add(baddatas);
                        errorMessages.Add($"Null value found in column '{badcolumns}'");
                        badRows = new List<string>();

                    }

                    // Data Type Validation
                    DataTable validDataTypesDataTable = validRowsDataTable.Clone();

                    for (int row = 0; row < validRowsDataTable.Rows.Count; row++)
                    {
                        bool rowValidationFailed = false; // Flag to track row validation

                        for (int col = 0; col < validRowsDataTable.Columns.Count - 2; col++)
                        {
                            string cellData = validRowsDataTable.Rows[row][col].ToString();
                            EntityColumnDTO columnDTO = columnsDTO[col];
                        }
                        // If row validation succeeded, add the entire row to the validRowsDataTable
                        if (!rowValidationFailed)
                        {
                            validDataTypesDataTable.Rows.Add(validRowsDataTable.Rows[row].ItemArray);
                        }
                        // If row validation failed, add the entire row data as a comma-separated string to the badRows list
                        if (rowValidationFailed)
                        {
                            string badRow = string.Join("!", validRowsDataTable.Rows[row].ItemArray); // Join the row data with commas
                            badRows.Add(badRow);
                        }
                    }
                    //Primary Key Validation
                    List<int> primaryKeyColumns = new List<int>();
                    for (int col = 0; col < validDataTypesDataTable.Columns.Count - 2; col++)
                    {
                        EntityColumnDTO columnDTO = columnsDTO[col];
                        if (columnDTO.ColumnPrimaryKey)
                        {
                            primaryKeyColumns.Add(col);
                        }
                    }
                    HashSet<string> seenValues = new HashSet<string>(); // To store values in primary key columns for duplicate checking
                    var ids = await _excelService.GetAllIdsFromDynamicTable(tableName);//primary key id pass
                    for (int row = 0; row < validRowsDataTable.Rows.Count; row++)
                    {
                        bool rowValidationFailed = false; // Flag to track row validation
                        var badRowData = new List<string>(); // Store the row data for potential duplicate errors
                        for (int col = 0; col < primaryKeyColumns.Count; col++)
                        {
                            int primaryKeyColumnIndex = primaryKeyColumns[col];
                            string cellData = validRowsDataTable.Rows[row][primaryKeyColumnIndex].ToString();
                            if (ids.Contains(int.Parse(cellData)) || seenValues.Contains(cellData))
                            {
                                rowValidationFailed = true;
                                columnName = columnsDTO[primaryKeyColumnIndex].EntityColumnName;
                                badRows.Add(string.Join("!", validRowsDataTable.Rows[row].ItemArray)); // Store the row data
                                break;
                            }
                            if (seenValues.Contains(cellData))
                            {
                                // Set the flag to indicate validation failure for this row
                                rowValidationFailed = true;
                                break; // Exit the loop as soon as a validation failure is encountered
                            }
                            // Store the value for duplicate checking
                            seenValues.Add(cellData);
                        }
                        if (!rowValidationFailed)
                        {
                            successdata.Rows.Add(validRowsDataTable.Rows[row].ItemArray);
                        }
                        if (rowValidationFailed)
                        {
                            badRows.Add(string.Join("!", badRowData));
                        }
                    }
                }
                if (badRows.Count > 0)
                {
                    badRows = badRows.Where(x => x != "").ToList();
                    string values = string.Join(",", badRows.Select(row => row.Split(',').Last()));
                    ErrorRowNumber.Add(values);
                    badRows.Insert(0, comma_separated_string);
                    List<string> modifiedRows = badRows.Select(row =>
                    {
                        int lastCommaIndex = row.LastIndexOf(',');
                        if (lastCommaIndex >= 0)
                        {
                            return row.Substring(0, lastCommaIndex);
                        }
                        else
                        {
                            return row; // No comma found, keep the original string
                        }
                    }).Where(row => !string.IsNullOrEmpty(row)).ToList();
                    badRows = modifiedRows;
                    string delimiter = ";"; // Specify the delimiter you want
                    string baddatas = string.Join(delimiter, badRows);
                    filedatas.Add(baddatas);
                    errorMessages.Add($"Duplicate key value violates unique constraints in column '{columnName}' in {tableName}");
                    badRows = new List<string>();
                }

                //store log data


                var result = await _excelService.Createlog(tableName, filedatas, fileName, successdata.Rows.Count, errorMessages, total_count,ErrorRowNumber);

                // Build the values for the SQL INSERT statement
                _excelService.InsertDataFromDataTableToPostgreSQL(successdata, tableName, columns, file);
                if (successdata.Rows.Count == 0)
                {
                    _response.Result = result;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessage.Add("All Records are incorrect");
                    return Ok(_response);
                }
                else if (filedatas.Count == 0)
                {
                    _response.Result = result;
                    _response.StatusCode = HttpStatusCode.Created;
                    _response.IsSuccess = true;
                    _response.ErrorMessage.Add("All records are successfully stored");
                    return Ok(_response);
                }
                else
                {
                    _response.Result = result;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = true;
                    _response.ErrorMessage.Add("Passcount records are successfully stored failcount records are incorrect ");
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                string[] errorParts = ex.Message.Split(':');
                if (errorParts.Length >= 2)
                {
                    string[] errorMessageParts = errorParts[1].Split('\n');
                    string errorMessage = errorMessageParts[0].Trim();
                    // Log the error message and details to your log table
                    errorMessages.Add(errorMessage); // Add the error message to the list

                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false
                    };
                    response.ErrorMessage = errorMessages; // Set the list of error messages
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
                    response.ErrorMessage = errorMessages; // Set the list of error messages
                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }
            }

        }
    }
}



