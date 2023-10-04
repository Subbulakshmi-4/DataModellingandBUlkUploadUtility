
﻿using DMU_Git.Data;
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


