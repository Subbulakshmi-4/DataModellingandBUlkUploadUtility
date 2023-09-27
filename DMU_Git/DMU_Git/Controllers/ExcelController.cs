using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;

namespace DMU_Git.Controllers
{
    [Route("api/excel")]
    [EnableCors("AllowAngularDev")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _excelService;

        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        }

        [HttpPost("generate")]
        public IActionResult GenerateExcelFile([FromBody] List<TableColumn> columns)
        {
            try
            {
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
        public IActionResult UploadTemplate(IFormFile file)
        {
            try
            {
                // Check if a file was provided
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided.");
                }

                // Process the uploaded file (e.g., save it to a location)
                // You can use a library like EPPlus to read the Excel file if needed

                // Respond with a success message or other relevant data
                return Ok("Template uploaded successfully.");
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
    }
}
