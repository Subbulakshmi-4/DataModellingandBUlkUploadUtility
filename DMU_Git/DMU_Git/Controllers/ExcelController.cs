using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using DMU_Git.Data;
using DMU_Git.Services.Interface;
using Npgsql;
using NpgsqlTypes;

namespace DMU_Git.Controllers
{
    //[Route("api/excel")]

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

        //[HttpPost("upload")]
        //public IActionResult UploadTemplate(IFormFile file)
        //{
        //    try
        //    {
        //        // Check if a file was provided
        //        if (file == null || file.Length == 0)
        //        {
        //            return BadRequest("No file provided.");
        //        }

        //        // Process the uploaded file (e.g., save it to a location)
        //        // You can use a library like EPPlus to read the Excel file if needed

        //        // Respond with a success message or other relevant data
        //        return Ok("Template uploaded successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        var apiResponse = new APIResponse
        //        {
        //            StatusCode = HttpStatusCode.InternalServerError,
        //            IsSuccess = false,
        //            ErrorMessage = new List<string> { ex.Message },
        //            Result = null
        //        };

        //        return StatusCode((int)HttpStatusCode.InternalServerError, apiResponse);
        //    }
        //}


        [HttpPost("upload")]
        //public IActionResult UploadFile(IFormFile file, [FromBody] UploadParamsDTO uploadParams)
        //{
        public IActionResult UploadFile(IFormFile file, string tableName)
        {
            //var mydatabasename = databaseName;,string databaseName
            var mytablername = tableName;
            if (file == null || file.Length == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
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
                        _response.ErrorMessage.Add("No data found in the Excel file.");
                        return BadRequest(_response);

                    }

                    //if (uploadParams == null || string.IsNullOrEmpty(uploadParams.TableName) || string.IsNullOrEmpty(uploadParams.DbName))
                    //return BadRequest("Table name and database name are required.");
                    //string tableName = uploadParams.TableName;
                    //string dbName = uploadParams.DbName;
                    if (string.IsNullOrEmpty(tableName))
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessage.Add("Table name is required.");
                        return BadRequest(_response);


                    }
                    //if (string.IsNullOrEmpty(databaseName))
                    //{
                    //    _response.StatusCode = HttpStatusCode.BadRequest;
                    //    _response.ErrorMessage.Add("database name is required.");
                    //    return BadRequest(_response);


                    //}

                    // Get the columns from the first row (assuming all rows have the same structure)
                    var columns = data.First().Keys.ToList();

                    // Build the values for the SQL INSERT statement
                    var values = data.Select(row =>
                        $"({string.Join(", ", columns.Select(col => $"'{row[col]}'"))})");


                    var insertQuery = $"INSERT INTO public.\"{mytablername}\" ({string.Join(", ", columns.Select(col => $"\"{col}\""))}) VALUES {string.Join(", ", values)}";

                    string connectionString = $"Host=localhost;Database=DMUtilityProject;Username=postgres;Password=GoodVibes";
                    //string connectionString = $"Host=localhost;Database={dbName};Username=postgres;Password=pos@sql";


                    using (var connection = new NpgsqlConnection(connectionString)) // Replace with  connection string
                    {
                        connection.Open();

                        using (var command = new NpgsqlCommand(insertQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    _response.StatusCode = HttpStatusCode.Created;
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
    }
}





