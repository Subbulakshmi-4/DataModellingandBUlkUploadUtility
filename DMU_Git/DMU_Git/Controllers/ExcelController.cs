using DMU_Git.Models.DTO;
using DMU_Git.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Cors;

namespace DMU_Git.Controllers
{
    [Route("api/excel")]
    [EnableCors("AllowAngular")]
    public class ExcelController : Controller
    {
        private readonly IExcelService excelService;

        public ExcelController(IExcelService excelService)
        {
            this.excelService = excelService;
        }

        [HttpPost("generate")]
        public IActionResult GenerateExcelFile([FromBody] List<TableColumn> columns)
        {
            try
            {
                byte[] excelBytes = excelService.GenerateExcelFile(columns);

                // Create a response for downloading the Excel file
                var fileContentResult = new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = "columns.xlsx"
                };

                return fileContentResult;
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new APIResponse<byte[]>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessage = new List<string> { ex.Message },
                    Result = null
                });
            }
        }
    }
}
