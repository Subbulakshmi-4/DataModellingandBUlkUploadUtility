using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;

using Microsoft.EntityFrameworkCore;

using Microsoft.Data.SqlClient.DataClassification;

using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

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
            worksheet.Cells[1, 6].Value = "Blank Not Allowed";
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
            worksheet.Cells[lastRowIndex + 3, 1].Value = "1. Don't add or delete any columns";
            worksheet.Cells[lastRowIndex + 4, 1].Value = "2. Don't add any extra sheets";
            worksheet.Cells[lastRowIndex + 5, 1].Value = "3. Follow the length if mentioned";

            // Apply yellow background color to the static content cells in the last row
            using (var staticContentRange = worksheet.Cells[lastRowIndex + 2, 1, lastRowIndex + 5, 5])
            {
                staticContentRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                staticContentRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            var columnNamesWorksheet = package.Workbook.Worksheets.Add("Column Names");

            // Add column names as headers horizontally in the second sheet
            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                columnNamesWorksheet.Cells[1, i + 1].Value = column.EntityColumnName;
            }

            var columnCount = columns.Count;

            // Loop through columns in "Column Names" worksheet and protect columns without headers
            for (int col = 1; col <= columnCount; col++)
            {
                if (string.IsNullOrWhiteSpace(columnNamesWorksheet.Cells[1, col].Text))
                {
                    columnNamesWorksheet.Column(col).Style.Locked = true;
                }
                else
                {
                    columnNamesWorksheet.Column(col).Style.Locked = false;
                }
            }

            // Apply data validation based on the data type to the "Column Names" sheet
            

            columnNamesWorksheet.Protection.IsProtected = true;
            columnNamesWorksheet.Protection.AllowSelectLockedCells = true;
            columnNamesWorksheet.Row(1).Style.Locked = true;

            // Create worksheets here

            // Apply custom data validation to the "Column Names" sheet
            //ApplyDataValidation(columns, worksheet, columnNamesWorksheet);


            return package.GetAsByteArray();
        }
    }


    public DataTable ReadExcelFromFormFile(IFormFile excelFile)
    {
        using (Stream stream = excelFile.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                DataTable dataTable = new DataTable();



                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];



                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    dataTable.Columns.Add(firstRowCell.Text);
                }



                for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                    var dataRow = dataTable.NewRow();
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        dataRow[col - 1] = row[rowNumber, col].Text;
                    }
                    dataTable.Rows.Add(dataRow);
                }



                return dataTable;
            }
        }
    }

    //private void ApplyDataValidation(List<EntityColumnDTO> columns, ExcelWorksheet columnsWorksheet, ExcelWorksheet columnNamesWorksheet)
    //{
    //    for (int row = 2; row <= columnNamesWorksheet.Dimension.End.Row; row++)
    //    {
    //        var cell = columnNamesWorksheet.Cells[row, 1];
    //        var columnName = cell.Text;

    //        // Find the corresponding column in the "Columns" worksheet
    //        var column = columns.FirstOrDefault(c => c.EntityColumnName == columnName);

    //        if (column != null)
    //        {
    //            var dataType = column.Datatype.ToLower();
    //            var valueCell = columnNamesWorksheet.Cells[row, 2];
    //            var length = column.Length;

    //            if (dataType == "integer")
    //            {
    //                // Set data validation for data type
    //                var dataValidation = valueCell.DataValidation.AddIntegerDataValidation();
    //                //dataValidation.Formula = 0; // Minimum value
    //                //dataValidation.Formula2 = 1000; // Maximum value
    //                dataValidation.ShowInputMessage = true;
    //                dataValidation.PromptTitle = "Data Type Error";
    //                dataValidation.Prompt = "Data type should be an integer.";

    //                // Set custom error message
    //                dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
    //                dataValidation.ErrorTitle = "Data Type Error";
    //                dataValidation.Error = "Data type should be an integer.";
    //            }
    //            else if (dataType == "string")
    //            {
    //                // Set data validation for string length
    //                var dataValidation = valueCell.DataValidation.AddTextLengthDataValidation();
    //                //dataValidation.Formula.Value = length.ToString();
    //                dataValidation.ShowInputMessage = true;
    //                dataValidation.PromptTitle = "Length Error";
    //                dataValidation.Prompt = $"Text length should be less than or equal to {length} characters.";

    //                // Set custom error message
    //                dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
    //                dataValidation.ErrorTitle = "Length Error";
    //                dataValidation.Error = $"Text length should be less than or equal to {length} characters.";
    //            }
    //        }
    //    }
    //}

    //private void ApplyDataValidation(List<EntityColumnDTO> columns, ExcelWorksheet columnsWorksheet, ExcelWorksheet columnNamesWorksheet)
    //{
    //    for (int row = 2; row <= columnNamesWorksheet.Dimension.End.Row; row++)
    //    {
    //        var cell = columnNamesWorksheet.Cells[row, 1];
    //        var columnName = cell.Text;

    //        // Find the corresponding column in the "Columns" worksheet
    //        var column = columns.FirstOrDefault(c => c.EntityColumnName == columnName);

    //        if (column != null)
    //        {
    //            var dataType = column.Datatype.ToLower();
    //            var valueCell = columnNamesWorksheet.Cells[row, 2];
    //            var length = column.Length;

    //            if (dataType == "integer")
    //            {
    //                // Set data validation for data type (integer)
    //                var dataValidation = valueCell.DataValidation.AddIntegerDataValidation();
    //                dataValidation.ShowErrorMessage = true;
    //                dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
    //                dataValidation.ErrorTitle = "Data Type Error";
    //                dataValidation.Error = "Data type should be an integer.";
    //            }
    //            else if (dataType == "string")
    //            {
    //                // Set data validation for string length
    //                var dataValidation = valueCell.DataValidation.AddTextLengthDataValidation();
    //                dataValidation.Formula.Value = length;
    //                dataValidation.ShowErrorMessage = true;
    //                dataValidation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
    //                dataValidation.ErrorTitle = "Length Error";
    //                dataValidation.Error = $"Text length should be less than or equal to {length} characters.";
    //            }
    //        }
    //    }
    //}






    public List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream)
    {
        using (var package = new ExcelPackage(excelFileStream))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
            // handle sheet out range eception


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


    public bool IsValidDataType(string data, string expectedDataType)
    {
        switch (expectedDataType.ToLower())
        {
            case "string":
                return true; // For a "string" data type, any non-null string is valid.
            case "int":
                int intResult;
                return int.TryParse(data, out intResult); // Check if the data can be parsed as an integer.
            case "boolean":
                if (data.Equals("1") || data.Equals("0"))
                {
                    return true; // Data is a valid boolean (1 or 0).
                }
                bool boolResult;
                return bool.TryParse(data, out boolResult); // Check if the data can be parsed as a boolean.
            case "date":
                DateTime dateResult;
                return DateTime.TryParse(data, out dateResult); // Check if the data can be parsed as a date.
            case "bytea":
                return IsValidByteA(data); // Check if the data is a valid bytea.
            default:
                return false; // Unknown data type; you can adjust this logic accordingly.
        }
    }

    public bool IsValidByteA(string data)
    {
        // Assuming that the data is represented as a hexadecimal string,
        // you can check if it's a valid hexadecimal representation.
        if (IsHexString(data))
        {
            try
            {
                // Convert the hexadecimal string to bytes
                byte[] bytes = HexStringToBytes(data);



                // You can add additional checks here if necessary
                // For example, check if the byte array is not empty or within a specific length range.



                return true;
            }
            catch (Exception)
            {
                // An exception occurred during hex string to byte conversion, indicating invalid data.
                return false;
            }
        }



        return false;
    }

    public bool IsHexString(string input)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(input, @"\A\b[0-9a-fA-F]+\b\Z");
    }
    public byte[] HexStringToBytes(string hex)
    {
        int length = hex.Length / 2;
        byte[] bytes = new byte[length];
        for (int i = 0; i < length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    public IEnumerable<EntityColumnDTO> GetColumnsForEntity(string entityName)
    {
        var entity = _context.EntityListMetadataModels.FirstOrDefault(e => e.EntityName == entityName);

        if (entity == null)
        {
            // Entity not found, return a 404 Not Found response
            return null;
        }

        var columnsDTO = _context.EntityColumnListMetadataModels
            .Where(column => column.EntityId == entity.Id)
            .Select(column => new EntityColumnDTO
            {
                Id = column.Id,
                EntityColumnName = column.EntityColumnName,
                Datatype = column.Datatype,
                Length = column.Length,
                Description = column.Description,
                IsNullable = column.IsNullable,
                DefaultValue = column.DefaultValue,
                ColumnPrimaryKey = column.ColumnPrimaryKey
            }).ToList();

        if (columnsDTO.Count == 0)
        {
            // No columns found, return a 404 Not Found response with an error message
            return null;
        }

        return columnsDTO;
    }

    public async Task<LogDTO> Createlog(string tableName, List<string> filedata,string fileName, DataTable successdata)
    {
        var storeentity = await _context.EntityListMetadataModels.FirstOrDefaultAsync(x => x.EntityName.ToLower() == tableName.ToLower());
        LogParent logParent = new LogParent();
        logParent.FileName = fileName;
        logParent.User_Id = 1;
        logParent.Entity_Id = storeentity.Id;   
        logParent.Timestamp = DateTime.Now;
        logParent.FailCount = filedata.Count;
        logParent.PassCount = successdata.Rows.Count;
        logParent.RecordCount = logParent.FailCount + logParent.PassCount;

        // Insert the LogParent record
        await _context.logParents.AddAsync(logParent);
        await _context.SaveChangesAsync(); // Save changes to get the generated ParentId



        // Now, you can access the generated ParentId
        int parentId = logParent.ID; // Adjust this based on your actual property name
        string delimiter = ";"; // Specify the delimiter you want

        string result = string.Join(delimiter, filedata);


        LogChild logChild = new LogChild();
        logChild.ParentID = parentId; // Set the ParentId
        logChild.Filedata = result; // Set the values as needed
        logChild.ErrorMessage = "Datatype validation failed"; // Set the values as needed



        // Insert the LogChild record
        await _context.logChilds.AddAsync(logChild);
        await _context.SaveChangesAsync(); // Save changes for the LogChild record

        LogDTO logDTO = new LogDTO() { 
        LogParentDTOs = logParent,
        ChildrenDTOs = new List<LogChild>()
        {
            logChild 
        }
        };

       return logDTO;
        

    }
}


}

