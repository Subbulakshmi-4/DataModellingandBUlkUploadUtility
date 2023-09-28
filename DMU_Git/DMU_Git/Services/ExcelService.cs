using DMU_Git.Models;
using DMU_Git.Models.DTO;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;


public interface IExcelService
{
    byte[] GenerateExcelFile(List<EntityColumnDTO> columns);
}

public class ExcelService : IExcelService
{
    public byte[] GenerateExcelFile(List<EntityColumnDTO> columns)
    {
        using (var package = new ExcelPackage())
        {
            // Add a worksheet and populate it
            var worksheet = package.Workbook.Worksheets.Add("Columns");

            // Add column headers from the columns list
            for (int i = 0; i < columns.Count; i++)
            {
                // Assuming columns[i].EntityColumnName contains the column name
                worksheet.Cells[1, i + 1].Value = columns[i].EntityColumnName;
            }

            return package.GetAsByteArray();
        }
    }

}
