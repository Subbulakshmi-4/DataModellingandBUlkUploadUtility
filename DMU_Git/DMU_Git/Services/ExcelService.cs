using DMU_Git.Data;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OfficeOpenXml;
using System.Data;
using Spire.Xls;
using DMU_Git.Models;
using Dapper;
using System.Text;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using DMU_Git.Services;
using Azure;
using Microsoft.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Buffers;
using Microsoft.IdentityModel.Tokens;
using static OfficeOpenXml.ExcelErrorValue;



public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;
    private readonly IDbConnection _dbConnection;
    private readonly ExportExcelService _exportExcelService;
    public ExcelService(ApplicationDbContext context, IDbConnection dbConnection, ExportExcelService exportExcelService)
    {
        _context = context;
        _dbConnection = dbConnection;
        _exportExcelService = exportExcelService;
    }
    public byte[] GenerateExcelFile(List<EntityColumnDTO> columns, int? parentId)
    {
        Workbook workbook = new Workbook();
        Worksheet worksheet = workbook.Worksheets[0];
        
        // Add the first worksheet with detailed column information
        worksheet.Name = "DataDictionary";
        worksheet.DefaultColumnWidth = 20;

        // Set protection options for the first sheet (read-only)
        worksheet.Protect("your_password", SheetProtectionType.All);
        worksheet.Protect("your_password", SheetProtectionType.None);
        // Add column headers for the first sheet
        worksheet.Range["A2"].Text = "SI.No";
        worksheet.Range["B2"].Text = "Data Item";
        worksheet.Range["C2"].Text = "Data Type";
        worksheet.Range["D2"].Text = "Length";
        worksheet.Range["E2"].Text = "MinLength";
        worksheet.Range["F2"].Text = "MaxLength";
        worksheet.Range["G2"].Text = "DateMinValue";
        worksheet.Range["H2"].Text = "DateMaxValue";
        worksheet.Range["I2"].Text = "Description";
        worksheet.Range["J2"].Text = "Blank Not Allowed";
        worksheet.Range["K2"].Text = "Default Value";
        worksheet.Range["L2"].Text = "Unique Value";
        worksheet.Range["M2"].Text = "Option1";
        worksheet.Range["N2"].Text = "Option2";
        // Populate the first sheet with column details
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            worksheet.Range[i + 3, 1].Value = (i+1).ToString();
            worksheet.Range[i + 3, 2].Text = column.EntityColumnName;
            worksheet.Range[i + 3, 3].Text = column.Datatype;
            worksheet.Range[i + 3, 4].Text = string.IsNullOrEmpty(column.Length.ToString()) || column.Length.ToString() == "0".ToString() ? string.Empty : column.Length.ToString();
            if (column.MinLength == null || column.MinLength == 0)
            {
                worksheet.Range[i + 3, 5].Text = string.Empty;
            }
            else
            {
                worksheet.Range[i + 3, 5].Text = column.MinLength.ToString();
            }

            if (column.MaxLength == 0)
            {
                worksheet.Range[i + 3, 6].Text = string.Empty;
            }
            else
            {
                worksheet.Range[i + 3, 6].Text = column.MaxLength.ToString();
            }



            if (string.IsNullOrEmpty(column.DateMinValue) && string.IsNullOrEmpty(column.DateMaxValue))
            {
                worksheet.Range[i + 3, 7].Text = string.Empty;
                worksheet.Range[i + 3, 8].Text = string.Empty;
            }
            else
            {
                worksheet.Range[i + 3, 7].Text = column.DateMinValue;
                worksheet.Range[i + 3, 8].Text = column.DateMaxValue;
            }
            worksheet.Range[i + 3, 8].Text = column.DateMaxValue.ToString();
            worksheet.Range[i + 3, 9].Text = column.Description;
            worksheet.Range[i + 3, 10].Text = column.IsNullable.ToString();
            if (column.Datatype.ToLower() == "boolean")
            {
                if (column.DefaultValue.ToLower() == "true")
                {
                    worksheet.Range[i + 3, 11].Text = column.True;
                }
                else if (column.DefaultValue.ToLower() == "false")
                {
                    worksheet.Range[i + 3, 11].Text = column.False;                                                                                                                                                                                                                                                                             
                }
            }
            else
            {
                worksheet.Range[i + 3, 11].Text = column.DefaultValue.ToString();
            }
            worksheet.Range[i + 3, 12].Text = column.ColumnPrimaryKey.ToString();
            worksheet.Range[i + 3, 13].Text = column.True.ToString();
            worksheet.Range[i + 3, 14].Text = column.False.ToString();
            var lastRowIndex1 = worksheet.Rows.Length;
            worksheet.Range[lastRowIndex1 + 1, 1].Text = (i + 2).ToString();
            worksheet.Range[lastRowIndex1 + 1, 1].Style.HorizontalAlignment = HorizontalAlignType.Right;
            worksheet.Range[lastRowIndex1 + 1, 2].Text = "CurrentDate";
            worksheet.Range[lastRowIndex1 + 1, 3].Text = "Date";
            int entityId = GetEntityIdByEntityName(column.entityname);
            worksheet.Range["A1"].Text = entityId.ToString();
        }
        worksheet.HideRow(1);
        // Add static content in the last row (vertically)
        var lastRowIndex = worksheet.Rows.Length;
        worksheet.Range[lastRowIndex + 2, 1].Text = "";
        worksheet.Range[lastRowIndex + 3, 1].Text = "Note:";
        worksheet.Range[lastRowIndex + 4, 1].Text = "1. Don't add or delete any columns";
        worksheet.Range[lastRowIndex + 5, 1].Text = "2. Don't add any extra sheets";
        worksheet.Range[lastRowIndex + 6, 1].Text = "3. Follow the length if mentioned";
        worksheet.Range[lastRowIndex + 7, 1].Text = "4. Current date column will be automatically updated. No need to fill that.";
        if (parentId.HasValue)
        {
            worksheet.Range[lastRowIndex + 7, 1].Text = "4. This is Exported Data ExcelFile";
            worksheet.Range[lastRowIndex + 8, 1].Text = "5. Before Upload the File remove the ErrorMessage";
        }
        var staticContentRange = worksheet.Range[lastRowIndex + 2, 1, lastRowIndex + 8, 5];
        staticContentRange.Style.FillPattern = ExcelPatternType.Solid;
        staticContentRange.Style.KnownColor = ExcelColors.Yellow;
        // Add the second worksheet for column names
        Worksheet columnNamesWorksheet = workbook.Worksheets.Add("Fill data");

        // After adding content to the columns
        //columnNamesWorksheet.AllocatedRange.AutoFitColumns();
        // Set a default column width for the "Fill data" worksheet
        columnNamesWorksheet.DefaultColumnWidth = 20; // Set the width in characters (adjust as needed)

        int lastColumnIndex = columns.Count + 1;

        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnNamesWorksheet.Range[2, i + 1].Text = column.EntityColumnName;
            int entityId = GetEntityIdByEntityName(column.entityname);
            columnNamesWorksheet.Range["A1"].Text = entityId.ToString();
        }

        columnNamesWorksheet.Range[2, lastColumnIndex].Text = "CurrentDate";

        columnNamesWorksheet.HideRow(1);

        if (parentId.HasValue)
        {
            InsertDataIntoExcel(columnNamesWorksheet, columns, parentId);
        }

        string[] sheetsToRemove = { "Sheet2", "Sheet3"}; // Names of sheets to be removed
        foreach (var sheetName in sheetsToRemove)
        {
            Worksheet sheetToRemove = workbook.Worksheets[sheetName];
            if (sheetToRemove != null)
            {
                workbook.Worksheets.Remove(sheetToRemove);
            }
        }
        if (parentId.HasValue)
        {
            Console.WriteLine("Excel is not validation");
        }
        else
        {
            AddDataValidation(columnNamesWorksheet, columns);
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
            workbook.SaveToStream(memoryStream, FileFormat.Version2013);
            return memoryStream.ToArray();
        }
    }

    private async Task InsertDataIntoExcel(Worksheet columnNamesWorksheet, List<EntityColumnDTO> columns, int? parentId)
    {
        try
        {
            var logChilds = await _exportExcelService.GetAllLogChildsByParentIDAsync(parentId.Value);
            int rowIndex = 3;

            foreach (var logChild in logChilds)
            {
                string[] rows = logChild.Filedata.Split(';');
                string errorMessage = logChild.ErrorMessage;

                for (int i = 1; i < rows.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(rows[i]))
                    {
                        continue;
                    }
                    string cleanedRow = rows[i].TrimStart(';').Trim();
                    string[] values = cleanedRow.Split(',');    //chng
                    for (int columnIndex = 0; columnIndex < values.Length; columnIndex++)
                    {
                        columnNamesWorksheet.Range[rowIndex, columnIndex + 1].Text = values[columnIndex];
                    }
                    columnNamesWorksheet.Range[rowIndex, values.Length + 1].Text = errorMessage;

                    rowIndex++;
                }
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private void HighlightDuplicates(Worksheet sheet, int columnNumber, int startRow, int endRow)
    {
        string columnLetter = GetExcelColumnName(columnNumber);

        string range = $"{columnLetter}{startRow}:{columnLetter}{endRow}";
        ConditionalFormatWrapper format = sheet.Range[range].ConditionalFormats.AddCondition();
        format.FormatType = ConditionalFormatType.DuplicateValues;
        format.BackColor = Color.IndianRed;
    }
    private void AddDataValidation(Worksheet columnNamesWorksheet, List<EntityColumnDTO> columns)
    {
        int startRow = 2; // The first row where you want validation
        int endRow = 100000;  // Adjust the last row as needed
        int columnCount = columnNamesWorksheet.Columns.Length - 1;
        char letter = 'A';
        char lastletter = 'A';
        // Protect the worksheet with a password
        columnNamesWorksheet.Protect("password");
        for (int i = 2; i <= columnCount; i++)
        {
            lastletter++;
        }
        for (int col = 1; col <= columnCount; col++)
        {
            // Get the data type for the current column
            string dataType = columns[col - 1].Datatype;
            int length = columns[col - 1].Length;
            bool isPrimaryKey = columns[col - 1].ColumnPrimaryKey;
            string truevalue = columns[col - 1].True;
            string falsevalue = columns[col - 1].False;
            bool isNullable = columns[col - 1].IsNullable;
            int? nullableMinRange = columns[col - 1].MinRange;
            int? nullableMaxRange = columns[col - 1].MaxRange;
            int minRange = nullableMinRange.HasValue ? nullableMinRange.Value : 0; // Use 0 as a default value when MinLength is null
            int maxRange = nullableMaxRange.HasValue ? nullableMaxRange.Value : 0; // Use 0 as a default value when MaxLength is null
            int? nullableMinLength = columns[col - 1].MinLength;
            int? nullableMaxLength = columns[col - 1].MaxLength;
            int minLength = nullableMinLength.HasValue ? nullableMinLength.Value : 0; // Use 0 as a default value when MinLength is null
            int maxLength = nullableMaxLength.HasValue ? nullableMaxLength.Value : 0; // Use 0 as a default value when MaxLength is null
            string dateMinValue = columns[col - 1].DateMinValue;
            string dateMaxValue = columns[col - 1].DateMaxValue;
            // Specify the range for data validation
            CellRange range = columnNamesWorksheet.Range[startRow, col, endRow, col];
            Validation validation = range.DataValidation;
            //Protect the worksheet with password
            columnNamesWorksheet.Protect("123456", SheetProtectionType.All);

            if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                // Text validation with min and max length
                validation.CompareOperator = ValidationComparisonOperator.Between;
                if ((minLength == 0) && (maxLength == 0))
                {
                    // Handle the case when both minimum and maximum length are 0
                    validation.Formula1 = "0";
                    validation.Formula2 = "0";
                }
                else if((!string.IsNullOrEmpty(minLength.ToString()) || minLength == 0) && (string.IsNullOrEmpty(maxLength.ToString()) || maxLength == 0))
                {
                    // Minimum length provided, no maximum length
                    validation.Formula1 = minLength.ToString();
                    validation.AllowType = CellDataType.TextLength;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Enter a value with a minimum length of {validation.Formula1} characters.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = $"The value must have a minimum length of {validation.Formula1} characters.";
                }
                else if ((string.IsNullOrEmpty(minLength.ToString()) || minLength == 0) && (!string.IsNullOrEmpty(maxLength.ToString()) || maxLength == 0))
                {
                    validation.Formula2 = maxLength.ToString();
                    validation.AllowType = CellDataType.TextLength;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Type text with a maximum length of {validation.Formula2} characters.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "The entered value exceeds the allowed length.";
                }
                else
                {
                    // Both minimum and maximum length provided
                    validation.Formula1 = minLength.ToString();
                    validation.Formula2 = maxLength.ToString();
                    validation.AllowType = CellDataType.TextLength;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Type text with a length between {validation.Formula1} and {validation.Formula2} characters.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "Entered value should be within the specified length range.";
                }
                if (isPrimaryKey)
                {
                    HighlightDuplicates(columnNamesWorksheet, col, startRow, endRow);
                    validation.InputMessage = $"Enter the unique value.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "Entered Values must be unique";
                }
            }
            else if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                validation.CompareOperator = ValidationComparisonOperator.Between;
                if (isPrimaryKey)
                {
                    HighlightDuplicates(columnNamesWorksheet, col, startRow, endRow);

                    validation.CompareOperator = ValidationComparisonOperator.Between;
                    if ((minRange == 0) && (maxRange == 0))
                    {
                        validation.Formula1 = "1";
                        validation.Formula2 = int.MaxValue.ToString();
                        validation.AllowType = CellDataType.Integer;
                        validation.InputTitle = "Input Data";
                        validation.InputMessage = "Enter an integer from 1 ";
                        validation.ErrorTitle = "Error";
                        validation.ErrorMessage = "The value should be an greater than or equal to 1 ";
                    }
                    else if ((!string.IsNullOrEmpty(minRange.ToString()) || minRange == 0) && (string.IsNullOrEmpty(maxRange.ToString()) || maxRange == 0))
                    {
                        // Minimum value provided, no maximum value
                        validation.Formula1 = minRange.ToString();
                        validation.Formula2 = int.MaxValue.ToString();
                        validation.AllowType = CellDataType.Integer;
                        validation.InputTitle = "Input Data";
                        validation.InputMessage = $"Enter a value with a minimum value of {validation.Formula1}.";
                        validation.ErrorTitle = "Error";
                        validation.ErrorMessage = $"The value must be at least {validation.Formula1}.";
                    }
                    else if ((string.IsNullOrEmpty(minRange.ToString()) || minRange == 0) && (!string.IsNullOrEmpty(maxRange.ToString()) || maxRange == 0))
                    {
                        validation.Formula1 = "1";
                        validation.Formula2 = maxRange.ToString();
                        validation.AllowType = CellDataType.Integer;
                        validation.InputTitle = "Input Data";
                        validation.InputMessage = $"Enter an integer value between 1 to {validation.Formula2}.";
                        validation.ErrorTitle = "Error";
                        validation.ErrorMessage = "The entered value exceeds the allowed range.";
                    }
                    else
                    {
                        // Both minimum and maximum values provided
                        validation.Formula1 = minRange.ToString();
                        validation.Formula2 = maxRange.ToString();
                        validation.AllowType = CellDataType.Integer;
                        validation.InputTitle = "Input Data";
                        validation.InputMessage = $"Enter an integer between {validation.Formula1} and {validation.Formula2}.";
                        validation.ErrorTitle = "Error";
                        validation.ErrorMessage = "The value should be within the specified range.";
                    }
                }
                else if ((minRange == 0) && (maxRange == 0))
                {
                    // Handle the case when both minimum and maximum length are 0
                    validation.CompareOperator = ValidationComparisonOperator.Between;
                    validation.Formula1 = int.MinValue.ToString(); 
                    validation.Formula2 = int.MaxValue.ToString(); 
                    validation.AllowType = CellDataType.Integer;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = "Enter an integer.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "The value should be an integer ";
                }
                else if ((!string.IsNullOrEmpty(minRange.ToString()) || minRange == 0) && (string.IsNullOrEmpty(maxRange.ToString()) || maxRange == 0))
                {
                    // Minimum value provided, no maximum value
                    validation.Formula1 = minRange.ToString();
                    validation.Formula2 = int.MaxValue.ToString();
                    validation.AllowType = CellDataType.Integer;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Enter a value with a minimum value of {validation.Formula1}.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = $"The value must be at least {validation.Formula1}.";
                }
                else if ((string.IsNullOrEmpty(minRange.ToString()) || minRange == 0) && (!string.IsNullOrEmpty(maxRange.ToString()) || maxRange == 0))
                {
                    validation.Formula1 = int.MinValue.ToString();
                    validation.Formula2 = maxRange.ToString();
                    validation.AllowType = CellDataType.Integer;
                    validation.InputTitle = "Input Data";   
                    validation.InputMessage = $"Enter an integer value less than or equal to {validation.Formula2}.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "The entered value exceeds the allowed range.";
                }
                else
                {
                    // Both minimum and maximum values provided
                    validation.Formula1 = minRange.ToString();
                    validation.Formula2 = maxRange.ToString();
                    validation.AllowType = CellDataType.Integer;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Enter an integer between {validation.Formula1} and {validation.Formula2}.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "The value should be within the specified range.";
                }
                
            }
            else if (dataType.Equals("Date", StringComparison.OrdinalIgnoreCase))
            {
                // Date validation
                validation.CompareOperator = ValidationComparisonOperator.Between;

                if (string.IsNullOrEmpty(dateMinValue) && string.IsNullOrEmpty(dateMaxValue))
                {
                    // No minimum and maximum date values provided
                    validation.Formula1 = "1757-01-01";
                    validation.Formula2 = "9999-01-01";
                }
                else if (!string.IsNullOrEmpty(dateMinValue) && string.IsNullOrEmpty(dateMaxValue))
                {
                    // Minimum date value provided, no maximum date value
                    validation.Formula1 = dateMinValue;
                    validation.Formula2 = "9999-01-01";
                }
                else if (string.IsNullOrEmpty(dateMinValue) && !string.IsNullOrEmpty(dateMaxValue))
                {
                    // No minimum date value, maximum date value provided
                    validation.Formula1 = "1757-01-01";
                    validation.Formula2 = dateMaxValue;
                }
                else
                {
                    // Both minimum and maximum date values provided
                    validation.Formula1 = dateMinValue;
                    validation.Formula2 = dateMaxValue;
                }

                validation.AllowType = CellDataType.Date;
                validation.InputTitle = "Input Data";
                validation.InputMessage = $"Type a date between {validation.Formula1} and {validation.Formula2} in this cell.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid date with correct format (yyyy-MM-dd).";

                // Ensure the date format is not avoided
                var cellRange = range.Worksheet.Range[range.Row, range.Column];
                cellRange.NumberFormat = "yyyy-MM-dd";
            }
            else if (dataType.Equals("boolean", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(truevalue) && string.IsNullOrEmpty(falsevalue))
                {
                    List<string> booleanOptions = new List<string> { "True", "False" };

                    // No specific values provided, allow "true" and "false"
                    validation.Values = new string[] { "true", "false" };
                    validation.ErrorTitle = "Error";
                    validation.InputTitle = "Input Data";
                    validation.ErrorMessage = "Select values from dropdown";
                    validation.InputMessage = "Select values from dropdown";
                }
                else
                {
                    // Specific values provided, enforce dropdown validation
                    validation.Values = new string[] { truevalue, falsevalue };
                    validation.ErrorTitle = "Error";
                    validation.InputTitle = "Input Data";
                    validation.ErrorMessage = "Select values from dropdown";
                    validation.InputMessage = "Select values from dropdown";
                }
            }
            else if (dataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                validation.CompareOperator = ValidationComparisonOperator.Between; // You can use any operator here.
                validation.Formula1 = "01/01/1900";
                validation.Formula2 = "12/31/9999"; // Use dummy values since you're not restricting the range
                validation.AllowType = CellDataType.Date;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a date and time in the specified format(mm/dd/yyyy hh:mm AM/PM)";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid date and time.";
                var cellRange = range.Worksheet.Range[range.Row, range.Column];
                cellRange.NumberFormat = "mm/dd/yyyy hh:mm AM/PM"; //
            }
            else if (dataType.Equals("char", StringComparison.OrdinalIgnoreCase))
            {
                // Character validation for a single character
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "1";
                validation.Formula2 = "1";
                validation.AllowType = CellDataType.TextLength;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a single character.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid character.";
            }
            else if (dataType.Equals("bytea", StringComparison.OrdinalIgnoreCase))
            {
                // Byte validation
                // Modify the validation code for bytea data
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "1"; // Set a minimum length of 1
                validation.Formula2 = "1000000"; // Set a maximum length as needed
                validation.AllowType = CellDataType.TextLength;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a byte array with a length between 1 and 1000000 characters.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Invalid byte array length";
                // Include byte validation
                bool isValidByteA = IsValidByteA(columns[col - 1].DefaultValue, 1, 1000000); // Modify the length limits as needed
                if (!isValidByteA)
                {
                    // Data does not meet byte validation criteria
                    validation.ErrorMessage = "Invalid byte array format or length.";
                }
            }
        }
        for (int i = 3; i <= 65537; i++)
        {
            string startindex = letter + i.ToString();
            string endindex = lastletter + i.ToString();
            CellRange lockrange = columnNamesWorksheet.Range[startindex + ":" + endindex];
            lockrange.Style.Locked = false;
        }
    }

    private string GetExcelColumnName(int columnNumber)
    {
        int dividend = columnNumber;
        string columnName = string.Empty;
        while (dividend > 0)
        {
            int modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }
        return columnName;
    }
    private int GetEntityIdByEntityName(string entityName)
    {
        // Assuming you have a list of EntityListMetadataModel instances
        List<EntityListMetadataModel> entityListMetadataModels = GetEntityListMetadataModels(); // Implement this method to fetch your metadata models

        // Use LINQ to find the entity Id
        int entityId = entityListMetadataModels
            .Where(model => model.EntityName == entityName)
            .Select(model => model.Id)
            .FirstOrDefault();

        if (entityId != 0) // Check if a valid entity Id was found
        {
            return entityId;
        }
        else
        {
            // Handle the case where the entity name is not found
            throw new Exception("Entity not found");
        }
    }
    private List<EntityListMetadataModel> GetEntityListMetadataModels()
    {
        {
            // Assuming YourDbContext is the Entity Framework DbContext for your database
            List<EntityListMetadataModel> entityListMetadataModels = _context.EntityListMetadataModels.ToList();
            return entityListMetadataModels;
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
                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    // Check the first cell in each column
                    //[2, col]=>[row,col]
                    var firstCell = worksheet.Cells[2, col];
                    if (string.IsNullOrWhiteSpace(firstCell.Text))
                    {
                        // Skip this column
                        continue;
                    }
                    dataTable.Columns.Add(firstCell.Text);
                    
                }
                dataTable.Columns.Add("RowNumber", typeof(int)); // Add "RowNumber" column
                for (int rowNumber = 3; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var dataRow = dataTable.NewRow();
                 // Set the "RowNumber" value for each row
                 //dataRow["RowNumber"] = rowNumber;
                    int colIndex = 0;
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        // Check if this column should be included
                        if (dataTable.Columns.Contains(worksheet.Cells[2, col].Text))
                        {
                            dataRow[colIndex] = worksheet.Cells[rowNumber, col].Text;
                            colIndex++;
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }
                bool allRowsAreNull = dataTable.AsEnumerable()
                .All(row => row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field.ToString())));
                if (allRowsAreNull)
                {
                    return null;
                }
                dataTable = dataTable.AsEnumerable()
                    .Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field.ToString())))
                    .CopyToDataTable();

                dataTable = dataTable.AsEnumerable().Select((row, index) =>
                {
                    row.SetField("RowNumber", index + 3);
                    return row;
                }).CopyToDataTable();


                return dataTable;
            }
        }
    }
    public List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream, int rowCount)
    {
        using (var package = new ExcelPackage(excelFileStream))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
            rowCount = rowCount + 2;
            int colCount = worksheet.Dimension.Columns;
            var data = new List<Dictionary<string, string>>();
            var columnNames = new List<string>();
            var skipColumns = new List<bool>();
            for (int col = 1; col <= colCount; col++)
            {
                var columnName = worksheet.Cells[2, col].Value?.ToString();
                columnNames.Add(columnName);
                // Check if the first cell in this column is empty or null
                skipColumns.Add(string.IsNullOrWhiteSpace(columnName));
            }
     
            // Read data rows
            for (int row = 3; row <= rowCount; row++)
            {
            
                var rowData = new Dictionary<string, string>();
                
                for (int col = 1; col <= colCount; col++)
                {
                    // If the column should be skipped, don't include it in the rowData
                    if (!skipColumns[col - 1])
                    {
                        var columnName = columnNames[col - 1];
                        var cellValue = worksheet.Cells[row, col].Value?.ToString();
                        rowData[columnName] = cellValue;
                    }
                }
                // Include the row number as "RowNumber" in the dictionary
                rowData["RowNumber"] = row.ToString();
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
    public bool IsValidByteA(string data, int minLength, int maxLength)
    {
        // Check if the input is a valid hexadecimal string
        if (!IsHexString(data))
        {
            return false;
        }

        // Check if the length is within acceptable limits
        if (data.Length < minLength || data.Length > maxLength)
        {
            return false;
        }

        // Add more specific checks if needed

        return true;
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
                ColumnPrimaryKey = column.ColumnPrimaryKey,
                True = column.True,
                False = column.False
            }).ToList();
        if (columnsDTO.Count == 0)
        {
            // No columns found, return a 404 Not Found response with an error message
            return null;
        }
        return columnsDTO;
    }

    public async Task<LogDTO> Createlog(string tableName, List<string> filedata, string fileName, int successdata, List<string> errorMessage, int total_count, List<string> ErrorRowNumber)
    {
        var storeentity = await _context.EntityListMetadataModels.FirstOrDefaultAsync(x => x.EntityName.ToLower() == tableName.ToLower());
        LogParent logParent = new LogParent();
        logParent.FileName = fileName;
        logParent.User_Id = 1;
        logParent.Entity_Id = storeentity.Id;
        logParent.Timestamp = DateTime.UtcNow;
        logParent.PassCount = successdata;
        logParent.RecordCount = total_count;
        logParent.FailCount = total_count - successdata;


        // Insert the LogParent record
        _context.logParents.Add(logParent);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            Console.WriteLine("Error: " + ex.Message);
        }

        List<LogChild> logChildren = new List<LogChild>();

        for (int i = 0; i < errorMessage.Count; i++)
        {
            LogChild logChild = new LogChild();
            logChild.ParentID = logParent.ID; // Set the ParentId
            logChild.ErrorMessage = errorMessage[i];

            if (filedata.Count > 0)
            {
                logChild.Filedata = filedata[i];
            }
            else
            {
                logChild.Filedata = ""; // Set the filedata as needed
            }

            if (ErrorRowNumber.Count > 0)
            {
                logChild.ErrorRowNumber = ErrorRowNumber[i];
            }
            else
            {
                logChild.ErrorRowNumber = ""; // Set the filedata as needed
            }
          
            // Insert the LogChild record
            _context.logChilds.Add(logChild);

            logChildren.Add(logChild);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            Console.WriteLine("Error: " + ex.Message);
        }

        LogDTO logDTO = new LogDTO()
        {
            LogParentDTOs = logParent,
            ChildrenDTOs = logChildren
        };

        return logDTO;
    }

    public void InsertDataFromDataTableToPostgreSQL(DataTable data, string tableName, List<string> columns, IFormFile file)
    {
        var columnProperties = GetColumnsForEntity(tableName).ToList();
        var booleancolumns = columnProperties.Where(c => c.Datatype.ToLower() == "boolean").ToList();
        List<Dictionary<string, string>> convertedDataList = new List<Dictionary<string, string>>();
        foreach (DataRow row in data.Rows)
        {
            Dictionary<string, string> convertedData = new Dictionary<string, string>();
            for (int i = 0; i < row.ItemArray.Length; i++)
            {
                string cellValue = row[i].ToString();
                EntityColumnDTO columnProperty = columnProperties.FirstOrDefault(col => col.EntityColumnName == data.Columns[i].ColumnName);
                if (columnProperty != null)
                {
                    // Use the column name from ColumnProperties as the key and the cell value as the value
                    convertedData[columnProperty.EntityColumnName] = cellValue;
                }
            }
            convertedDataList.Add(convertedData);
        }
        // 'convertedDataList' is now a list of dictionaries, each representing a row in the desired format.
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json"); 
        var storeentity = _context.EntityListMetadataModels.FirstOrDefaultAsync(x => x.EntityName.ToLower() == tableName.ToLower());
        tableName = storeentity.Result.EntityName;
        IConfigurationRoot configuration = configurationBuilder.Build();
        string connectionString = configuration.GetConnectionString("DefaultConnection");
        var errorDataList = convertedDataList;
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            List<Dictionary<string, string>> dataToRemove = new List<Dictionary<string, string>>();
            try
            {
                foreach (var data2 in convertedDataList)
                {
                foreach (var boolvalue in booleancolumns)
                {
                    if (data2.ContainsKey(boolvalue.EntityColumnName))
                    {
                        // Update the value for the specific key
                        var value = data2[boolvalue.EntityColumnName];
                        if (value.ToLower() == boolvalue.True.ToLower())
                        {
                            data2[boolvalue.EntityColumnName] = "1";
                        }
                        else
                        {
                            data2[boolvalue.EntityColumnName] = "0";
                        }
                    }
                }
                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = connection;
                        // Build the INSERT statement
                        string columns2 = string.Join(", ", data2.Keys.Select(k => $"\"{k}\"")); // Use double quotes for case-sensitive column names
                        string values = string.Join(", ", data2.Values.Select(v => $"'{v}'")); // Wrap values in single quotes for strings
                        string query = $"INSERT INTO \"{tableName}\" ({columns2}) VALUES ({values})"; // Use double quotes for case-sensitive table name
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                        dataToRemove.Add(data2);
                    }
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                connection.Close();
                var successdata = convertedDataList.Count - errorDataList.Count;
                string errorMessages = "Server error";
                string successMessage = " ";
                string fileName = file.FileName;
                List<string> errorRownumber = new List<string>();
                List<string> badRows = new List<string>();
                foreach (var dataToRemoveItem in dataToRemove)
                {
                    errorDataList.Remove(dataToRemoveItem);
                }
                foreach (var dict in errorDataList)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var value in dict.Values)
                    {
                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(value);
                    }
                    badRows.Add(sb.ToString());
                }
                string comma_separated_string = string.Join(",", columns.ToArray());
                badRows.Insert(0, comma_separated_string);
                var result = Createlog(tableName, badRows, fileName, successdata, new List<string> { errorMessages }, convertedDataList.Count, errorRownumber);
               

            }
        }
    }
    public int GetEntityIdByEntityNamefromui(string entityName)
    {
        // Assuming you have a list of EntityListMetadataModel instances
        List<EntityListMetadataModel> entityListMetadataModels = GetEntityListMetadataModelforlist(); // Implement this method to fetch your metadata models

        // Use LINQ to find the entity Id
        int entityId = entityListMetadataModels
            .Where(model => model.EntityName == entityName)
            .Select(model => model.Id)
            .FirstOrDefault();

        if (entityId != 0) // Check if a valid entity Id was found
        {
            return entityId;
        }
        else
        {
            // Handle the case where the entity name is not found
            throw new Exception("Entity not found");//return null
        }
    }
    public List<EntityListMetadataModel> GetEntityListMetadataModelforlist()
    {
        {           
            List<EntityListMetadataModel> entityListMetadataModels = _context.EntityListMetadataModels.ToList();
            return entityListMetadataModels;
        }
    }
   
    public int? GetEntityIdFromTemplate(IFormFile file, int sheetIndex)
    {
        using (var package = new ExcelPackage(file.OpenReadStream()))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetIndex];
            int entityId;
            if (int.TryParse(worksheet.Cells[1, 1].Text, out entityId))
            {
                return entityId;
            }
            return null;
        }
    }

    public string GetPrimaryKeyColumnForEntity(string entityName)
    {
        var entity = _context.EntityListMetadataModels.FirstOrDefault(e => e.EntityName == entityName);
        if (entity == null)
        {
            // Entity not found, return null or throw an exception
            return null;
        }
        var primaryKeyColumn = _context.EntityColumnListMetadataModels
            .Where(column => column.EntityId == entity.Id && column.ColumnPrimaryKey)
            .Select(column => column.EntityColumnName)
            .FirstOrDefault();
        return primaryKeyColumn;
    }
    public async Task<List<int>> GetAllIdsFromDynamicTable(string tableName)
    {
        string primaryKeyColumn = GetPrimaryKeyColumnForEntity(tableName);
        try
        {
            // Use Dapper to execute a parameterized query to fetch IDs
            string query = $"SELECT \"{primaryKeyColumn}\" FROM public.\"{tableName}\";";
            var ids = await _dbConnection.QueryAsync<int>(query);
            return ids.ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching IDs from the specified table.", ex);
        }
    }
    public bool TableExists(string tableName)
    {
        try
        {
            // Use Dapper to execute a parameterized query to check if the table exists
            string query = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @TableName)";
            bool tableExists = _dbConnection.QueryFirstOrDefault<bool>(query, new { TableName = tableName });
            return tableExists;
        }
        catch (Exception ex)
        {
            throw new Exception("Error checking table existence in the specified database.", ex);
        }
    }
}


