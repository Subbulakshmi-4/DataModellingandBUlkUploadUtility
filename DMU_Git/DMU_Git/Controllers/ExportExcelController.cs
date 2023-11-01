using DMU_Git.Services;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.Net.Http.Headers;


namespace DMU_Git.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportExcelController : ControllerBase
    {
        private readonly ExportExcelService _exportExcelService;
        public ExportExcelController(ExportExcelService exportExcelService)
        {
            _exportExcelService = exportExcelService;
        }
        [HttpGet("{parentId}")]
        public async Task<IActionResult> ExportToExcel(int parentId)
        {
            var logChilds = await _exportExcelService.GetLogChildsByParentIDAsync(parentId);
            if (logChilds.Any())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("LogChildData");
                    int row = 2; 
                    foreach (var logChild in logChilds)
                    {
                        var filedataRows = logChild.Filedata.Split(';');

                        foreach (var filedataRow in filedataRows)
                        {
                            var cells = filedataRow.Split(',');
                            int col = 1;
                            foreach (var cellValue in cells)
                            {
                                worksheet.Cells[row, col].Value = cellValue;
                                col++;
                            }
                            row++;
                        }
                        worksheet.Cells[row, 1].Value = "ErrorMessage:" + " " + logChild.ErrorMessage;
                        row++;
                    }
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await package.SaveAsAsync(stream);
                        var content = stream.ToArray();
                        var fileName = $"Testing_LogChildData.xlsx";
                        var contentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        };
                        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        return File(content, Response.ContentType);
                    }
                }
            }
            else
            {
                return NotFound("LogChild data not found for the given ParentID.");
            }
        }
    }
}

