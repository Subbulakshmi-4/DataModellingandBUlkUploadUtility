using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;
    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public byte[] GenerateExcelFile(List<EntityColumnDTO> columns)

    {
        using (var package = new ExcelPackage())
        {
            // Add the first worksheet with detailed column information
            var worksheet = package.Workbook.Worksheets.Add("Columns");
            // Set protection options for the first sheet (read-only)
            worksheet.Protection.IsProtected = true;
            worksheet.Protection.AllowSelectLockedCells = true;

            // Add column headers for the first sheet
            worksheet.Cells[1, 1].Value = "SI.No";
            worksheet.Cells[1, 2].Value = "Data Item";
            worksheet.Cells[1, 3].Value = "Data Type";
            worksheet.Cells[1, 4].Value = "Length";
            worksheet.Cells[1, 5].Value = "Description";
            worksheet.Cells[1, 6].Value = "Blank Allowed";
            worksheet.Cells[1, 7].Value = "Default Value";
            worksheet.Cells[1, 8].Value = "Unique Value";


            // Populate the first sheet with column details
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                worksheet.Cells[i + 2, 1].Value = column.Id;
                worksheet.Cells[i + 2, 2].Value = column.EntityColumnName;
                worksheet.Cells[i + 2, 3].Value = column.Datatype;
                worksheet.Cells[i + 2, 4].Value = column.Length;
                worksheet.Cells[i + 2, 5].Value = column.Description;
                worksheet.Cells[i + 2, 6].Value = column.IsNullable;
                worksheet.Cells[i + 2, 7].Value = column.DefaultValue;
                worksheet.Cells[i + 2, 8].Value = column.ColumnPrimaryKey;
            }

            int lastRowIndex = worksheet.Dimension.End.Row;

            // Add static content in the last row (vertically)
            worksheet.Cells[lastRowIndex + 1, 1].Value = "";
            worksheet.Cells[lastRowIndex + 2, 1].Value = "Note:";
            worksheet.Cells[lastRowIndex + 3, 1].Value = "1.Don't add or delete any columns";
            worksheet.Cells[lastRowIndex + 4, 1].Value = "2.Don't add any extra sheets";
            worksheet.Cells[lastRowIndex + 5, 1].Value = "3.Follow the length if mentioned";
            // Add more static content as needed

            // Apply yellow background color to the static content cells in the last row
            using (var staticContentRange = worksheet.Cells[lastRowIndex + 2, 1, lastRowIndex + 5, 5])
            {
                staticContentRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                staticContentRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }
            // Add more static content as needed

            // Add the second worksheet with only column names
            var columnNamesWorksheet = package.Workbook.Worksheets.Add("Column Names");

            // Add column names as headers horizontally in the second sheet
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                columnNamesWorksheet.Cells[1, i + 1].Value = column.EntityColumnName;
            }

            return package.GetAsByteArray();
        }
    }

    public List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream)
    {
        using (var package = new ExcelPackage(excelFileStream))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];


            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            var data = new List<Dictionary<string, string>>();

            // Extract column names
            var columnNames = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var columnName = worksheet.Cells[1, col].Value?.ToString();
                columnNames.Add(columnName);
            }

            // Read data rows
            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, string>();
                for (int col = 1; col <= colCount; col++)
                {
                    var columnName = columnNames[col - 1];
                    var cellValue = worksheet.Cells[row, col].Value?.ToString();
                    rowData[columnName] = cellValue;
                }
                data.Add(rowData);
            }

            return data;
        }
    }



}


