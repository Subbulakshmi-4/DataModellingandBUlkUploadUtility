//using DMU_Git.Models;
//using DMU_Git.Services;
//using Microsoft.AspNetCore.Mvc;
//using OfficeOpenXml;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DMU_Git.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ExportExcelController : ControllerBase
//    {
//        private readonly ExportExcelService _exportExcelService;

//        public ExportExcelController(ExportExcelService exportExcelService)
//        {
//            _exportExcelService = exportExcelService;
//        }

//        [HttpGet("{parentID}")]
//        public async Task<IActionResult> ExportToExcel(int parentID)
//        {
//            var logChilds = await _exportExcelService.GetLogChildsByParentIDAsync(parentID);

//            if (logChilds.Any())
//            {
//                using (var package = new ExcelPackage())
//                {
//                    var worksheet = package.Workbook.Worksheets.Add("LogChildData");

//                    int row = 1; // Start from the first row

//                    // Data
//                    foreach (var logChild in logChilds)
//                    {
//                        // Split filedata into rows based on semicolons
//                        var filedataRows = logChild.Filedata.Split(';');

//                        foreach (var filedataRow in filedataRows)
//                        {
//                            // Split each row into cells based on commas
//                            var cells = filedataRow.Split(',');

//                            int col = 1; // Start from the first column

//                            // Set each cell value in the worksheet
//                            foreach (var cellValue in cells)
//                            {
//                                worksheet.Cells[row, col].Value = cellValue;
//                                col++;
//                            }

//                            // Move to the next row for the next filedata row
//                            row++;
//                        }

//                        // Set error message for the current logChild
//                        worksheet.Cells[row, 1].Value = "ErrorMessage:" + " " + logChild.ErrorMessage;

//                        // Move to the next row for the next logChild
//                        row++;
//                    }

//                    // Save the Excel package to a memory stream asynchronously
//                    using (MemoryStream stream = new MemoryStream())
//                    {
//                        await package.SaveAsAsync(stream);

//                        // Return the Excel file as a byte array
//                        var content = stream.ToArray();

//                        // Set response headers
//                        Response.Headers.Add("Content-Disposition", "attachment; filename=LogChildData.xlsx");
//                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

//                        // Return the Excel file as a byte array in the response body
//                        return File(content, Response.ContentType);
//                    }
//                }
//            }
//            else
//            {
//                return NotFound("LogChild data not found for the given ParentID.");
//            }
//        }
//    }
//}


using DMU_Git.Models;
using DMU_Git.Services;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpGet("{parentID}")]
        public async Task<IActionResult> ExportToExcel(int parentID, [FromQuery] string entityName)
        {
            var logChilds = await _exportExcelService.GetLogChildsByParentIDAsync(parentID);

            if (logChilds.Any())
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("LogChildData");

                    int row = 1; // Start from the first row

                    // Data
                    foreach (var logChild in logChilds)
                    {
                        // Split filedata into rows based on semicolons
                        var filedataRows = logChild.Filedata.Split(';');

                        foreach (var filedataRow in filedataRows)
                        {
                            // Split each row into cells based on commas
                            var cells = filedataRow.Split(',');

                            int col = 1; // Start from the first column

                            // Set each cell value in the worksheet
                            foreach (var cellValue in cells)
                            {
                                worksheet.Cells[row, col].Value = cellValue;
                                col++;
                            }

                            // Move to the next row for the next filedata row
                            row++;
                        }

                        // Set error message for the current logChild
                        worksheet.Cells[row, 1].Value = "ErrorMessage:" + " " + logChild.ErrorMessage;

                        // Move to the next row for the next logChild
                        row++;
                    }

                    // Save the Excel package to a memory stream asynchronously
                    using (MemoryStream stream = new MemoryStream())
                    {
                        await package.SaveAsAsync(stream);

                        // Return the Excel file as a byte array
                        var content = stream.ToArray();

                        // Set response headers
                        var fileName = $"{entityName}_LogChildData.xlsx";

                        var contentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = fileName
                        };
                        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                        // Return the Excel file as a byte array in the response body
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

