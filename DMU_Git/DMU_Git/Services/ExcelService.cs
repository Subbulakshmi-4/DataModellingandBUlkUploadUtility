using System;
using System.Collections.Generic;
using System.IO;
using DMU_Git.Data;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using Spire.Xls;
using Spire.Xls.Collections;
using Spire.Xls.Core;
using Spire.Xls.Core.Spreadsheet;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;

    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public byte[] GenerateExcelFile(List<EntityColumnDTO> columns)
    {

        Workbook workbook = new Workbook();
        Worksheet worksheet = workbook.Worksheets[0];

        // Add the first worksheet with detailed column information
        worksheet.Name = "Columns";

        // Set protection options for the first sheet (read-only)
        worksheet.Protect("your_password", SheetProtectionType.All);

        // Add column headers for the first sheet
        worksheet.Range["A1"].Text = "SI.No";
        worksheet.Range["B1"].Text = "Data Item";
        worksheet.Range["C1"].Text = "Data Type";
        worksheet.Range["D1"].Text = "Length";
        worksheet.Range["E1"].Text = "Description";
        worksheet.Range["F1"].Text = "Blank Not Allowed";
        worksheet.Range["G1"].Text = "Default Value";
        worksheet.Range["H1"].Text = "Unique Value";

        // Populate the first sheet with column details
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            worksheet.Range[i + 2, 1].Value = column.Id.ToString();
            worksheet.Range[i + 2, 2].Text = column.EntityColumnName;
            worksheet.Range[i + 2, 3].Text = column.Datatype;
            worksheet.Range[i + 2, 4].Text = column.Length.ToString();
            worksheet.Range[i + 2, 5].Text = column.Description;
            worksheet.Range[i + 2, 6].Text = column.IsNullable.ToString();
            worksheet.Range[i + 2, 7].Text = column.DefaultValue.ToString();
            worksheet.Range[i + 2, 8].Text = column.ColumnPrimaryKey.ToString();
        }

        // Add static content in the last row (vertically)
        var lastRowIndex = worksheet.Rows.Length;
        worksheet.Range[lastRowIndex + 1, 1].Text = "";
        worksheet.Range[lastRowIndex + 2, 1].Text = "Note:";
        worksheet.Range[lastRowIndex + 3, 1].Text = "1. Don't add or delete any columns";
        worksheet.Range[lastRowIndex + 4, 1].Text = "2. Don't add any extra sheets";
        worksheet.Range[lastRowIndex + 5, 1].Text = "3. Follow the length if mentioned";

        // Apply yellow background color to the static content cells in the last row
        var staticContentRange = worksheet.Range[lastRowIndex + 2, 1, lastRowIndex + 5, 5];
        staticContentRange.Style.FillPattern = ExcelPatternType.Solid;
        staticContentRange.Style.KnownColor = ExcelColors.Yellow;

        // Add the second worksheet for column names
        Worksheet columnNamesWorksheet = workbook.Worksheets.Add("Column Names");

        // Add column names as headers horizontally in the second sheet
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnNamesWorksheet.Range[1, i + 1].Text = column.EntityColumnName;
        }

        string[] sheetsToRemove = { "Sheet2", "Sheet3", "EvaluationWarning" }; // Names of sheets to be removed

        foreach (var sheetName in sheetsToRemove)
        {
            Worksheet sheetToRemove = workbook.Worksheets[sheetName];
            if (sheetToRemove != null)
            {
                workbook.Worksheets.Remove(sheetToRemove);
            }
        }
        // Loop through columns in "Column Names" worksheet and protect columns without headers
        var columnCount = columns.Count;
        // Apply data validation based on the data type to the "Column Names" sheet

        AddDataValidation(columnNamesWorksheet, columns);

        using (MemoryStream memoryStream = new MemoryStream())
        {
            workbook.SaveToStream(memoryStream, FileFormat.Version2013);
            return memoryStream.ToArray();
        }


    }

    private void AddDataValidation(Worksheet columnNamesWorksheet, List<EntityColumnDTO> columns)
    {
        int startRow = 2; // The first row where you want validation
        int endRow = 100000;  // Adjust the last row as needed
        int columnCount = columnNamesWorksheet.Columns.Length;

        for (int col = 1; col <= columnCount; col++)
        {

            // Get the data type for the current column
            string dataType = columns[col - 1].Datatype;

            int length = columns[col - 1].Length;
            bool isPrimaryKey = columns[col - 1].ColumnPrimaryKey;
            bool notNull = columns[col - 1].IsNullable;
            // Specify the range for data validation
            CellRange range = columnNamesWorksheet.Range[startRow, col, endRow, col];
            Validation validation = range.DataValidation;

            // Apply data validation based on data type

            if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                // Text validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "1";
                validation.Formula2 = length.ToString();  // Adjust the maximum text length as needed
                validation.AllowType = CellDataType.TextLength;
                validation.InputTitle = "Input Data";
                validation.InputMessage = $"Type text with a length between 1 and {length} characters.";
                validation.ErrorTitle = "Error001";
                if (isPrimaryKey = true)
                {

                    validation.InputMessage = "The value must be a unique string with a length between 1 and " + length + " characters.";

                }
            }
            else if (dataType.Equals("integer", StringComparison.OrdinalIgnoreCase))
            {
                // Number validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "1";
                validation.Formula2 = "1000000";  // Adjust the number range as needed
                validation.AllowType = CellDataType.Integer;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a number between 1 and 1,000,000 in this cell.";
                validation.ErrorTitle = "Error001";
            }
            else if (dataType.Equals("Date", StringComparison.OrdinalIgnoreCase))
            {
                // Date validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "01/01/1900";  // Adjust the minimum date as needed
                validation.Formula2 = "12/12/2023";  // Adjust the maximum date as needed
                validation.AllowType = CellDataType.Date;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a date between 01/01/1900 and 12/31/2100 in this cell.";
                validation.ErrorTitle = "Error001";
            }
            else if (dataType.Equals("boolean", StringComparison.OrdinalIgnoreCase))
            {
                // Data validation formula for "TRUE" or "FALSE"
               validation.Values = new string[] { "true", "false" };
                validation.ErrorTitle = "Error001";
                validation.InputMessage = "Please enter 'TRUE' or 'FALSE' in this cell.";
            }


            // Add more conditions for other data types as needed
        }
    }

    public List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream)
    {
        Workbook workbook = new Workbook();
        workbook.LoadFromStream(excelFileStream);
        Worksheet worksheet = workbook.Worksheets[0];

        int rowCount = worksheet.Rows.Length;
        int colCount = worksheet.Columns.Length;

        var data = new List<Dictionary<string, string>>();

        // Extract column names
        var columnNames = new List<string>();
        for (int col = 1; col <= colCount; col++)
        {
            var columnName = worksheet[1, col].Text;
            columnNames.Add(columnName);
        }

        // Read data rows
        for (int row = 2; row <= rowCount; row++)
        {
            var rowData = new Dictionary<string, string>();
            for (int col = 1; col <= colCount; col++)
            {
                var columnName = columnNames[col - 1];
                var cellValue = worksheet[row, col].Text;
                rowData[columnName] = cellValue;
            }
            data.Add(rowData);
        }

        return data;
    }
}
