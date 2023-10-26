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
using System.Net;
using Spire.Xls.Core;
using System.Drawing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Buffers;

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;
    private readonly IDbConnection _dbConnection;

    public ExcelService(ApplicationDbContext context, IDbConnection dbConnection)
    {
        _context = context;
        _dbConnection = dbConnection;
    }
        
    public byte[] GenerateExcelFile(List<EntityColumnDTO> columns)
    {
        Workbook workbook = new Workbook();
        Worksheet worksheet = workbook.Worksheets[0];
        
        // Add the first worksheet with detailed column information
        worksheet.Name = "DataDictionary";
            
        // Set protection options for the first sheet (read-only)
        worksheet.Protect("your_password", SheetProtectionType.All);
        worksheet.Protect("your_password", SheetProtectionType.None);



        // Add column headers for the first sheet

        worksheet.Range["A2"].Text = "SI.No";
        worksheet.Range["B2"].Text = "Data Item";
        worksheet.Range["C2"].Text = "Data Type";
        worksheet.Range["D2"].Text = "Length";
        worksheet.Range["E2"].Text = "Description";
        worksheet.Range["F2"].Text = "Blank Not Allowed";
        worksheet.Range["G2"].Text = "Default Value";
        worksheet.Range["H2"].Text = "Unique Value";
        worksheet.Range["I2"].Text = "Option1";
        worksheet.Range["J2"].Text = "Option2";

        // Populate the first sheet with column details
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            worksheet.Range[i + 3, 1].Value = (i+1).ToString();
            worksheet.Range[i + 3, 2].Text = column.EntityColumnName;
            worksheet.Range[i + 3, 3].Text = column.Datatype;
            worksheet.Range[i + 3, 4].Text = string.IsNullOrEmpty(column.Length.ToString()) || column.Length.ToString() == "0".ToString() ? string.Empty : column.Length.ToString();
            worksheet.Range[i + 3, 5].Text = column.Description;
            worksheet.Range[i + 3, 6].Text = column.IsNullable.ToString();
            if (column.Datatype.ToLower() == "boolean")
            {
                if (column.DefaultValue.ToLower() == "true")
                {
                    worksheet.Range[i + 3, 7].Text = column.True;
                }
                else if (column.DefaultValue.ToLower() == "false")
                {
                    worksheet.Range[i + 3, 7].Text = column.False;
                }
            }
            else
            {
                worksheet.Range[i + 3, 7].Text = column.DefaultValue.ToString();
            }
            worksheet.Range[i + 3, 8].Text = column.ColumnPrimaryKey.ToString();
            worksheet.Range[i + 3, 9].Text = column.True.ToString();
            worksheet.Range[i + 3, 10].Text = column.False.ToString();
            int entityId = GetEntityIdByEntityName(column.entityname);
            worksheet.Range["A1"].Text = entityId.ToString();
        }
        worksheet.HideRow(1);
       

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
        Worksheet columnNamesWorksheet = workbook.Worksheets.Add("Fill data");


        // Add column names as headers horizontally in the second sheet
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            columnNamesWorksheet.Range[1, i + 1].Text = column.EntityColumnName;
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
        AddDataValidation(columnNamesWorksheet, columns);
        
        using (MemoryStream memoryStream = new MemoryStream())
        {
            workbook.SaveToStream(memoryStream, FileFormat.Version2013);
            return memoryStream.ToArray();
        }
    }
    private void HighlightDuplicates(Worksheet sheet, int columnNumber, int startRow, int endRow)
    {
        // Convert the column number to a column letter (e.g., 1 => "A", 2 => "B")
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
        int columnCount = columnNamesWorksheet.Columns.Length;

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

            bool notNull = columns[col - 1].IsNullable;
            // Specify the range for data validation
            CellRange range = columnNamesWorksheet.Range[startRow, col, endRow, col];
            Validation validation = range.DataValidation;

            
            //Protect the worksheet with password
            columnNamesWorksheet.Protect("123456", SheetProtectionType.All);

            if (dataType.Equals("string", StringComparison.OrdinalIgnoreCase))
            {
                // Text validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                if (length > 0)
                {
                    validation.Formula1 = "1";
                    validation.Formula2 = length.ToString(); // Adjust the maximum text length as needed
                    validation.AllowType = CellDataType.TextLength;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = $"Type text with a length between 1 and {length} characters.";
                    validation.ErrorTitle = "Error";
                    if (isPrimaryKey)
                    {
                        validation.CompareOperator = ValidationComparisonOperator.Between;
                        validation.Formula1 = "1";  // Minimum length
                        validation.Formula2 = length.ToString(); // Maximum length
                        HighlightDuplicates(columnNamesWorksheet, col, startRow, endRow);
                        validation.InputTitle = "Input Data";
                        validation.InputMessage = "The value must be a unique string with a length between 1 and " + length + " characters.";
                        validation.ErrorTitle = "Error";
                        validation.ErrorMessage = "Values must be unique";
                    }
                }
                else
                {
                    // Skip length validation for length == 0
                    validation.Formula1 = "0"; // Set a minimum text length of 0
                    validation.Formula2 = "10000000";// Set a minimum text length of 1
                    validation.AllowType = CellDataType.TextLength;
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = "Enter the string";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "Entered value exceeds the length";

                }
            }
            if (dataType.Equals("int", StringComparison.OrdinalIgnoreCase))
            {
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "0"; // Minimum value (adjust as needed)
                validation.Formula2 = "1000000"; // Maximum value (adjust as needed)
                validation.AllowType = CellDataType.Integer;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type an integer between 0 and 1,000,000 in this cell.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid integer between 0 and 1,000,000.";

                if (isPrimaryKey)
                {
                    validation.CompareOperator = ValidationComparisonOperator.Between;
                    validation.Formula1 = "0"; // Minimum value for primary key
                    validation.Formula2 = "1000000"; // Maximum value for primary key
                    HighlightDuplicates(columnNamesWorksheet, col, startRow, endRow);
                    validation.InputTitle = "Input Data";
                    validation.InputMessage = "The value must be a unique integer between 0 and 1,000,000.";
                    validation.ErrorTitle = "Error";
                    validation.ErrorMessage = "Values must be unique integers within the specified range.";
                }
            }

            else if (dataType.Equals("Date", StringComparison.OrdinalIgnoreCase))
            {
                // Date validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "01/01/1900";  // Adjust the minimum date as needed
                validation.Formula2 = "12/12/2023";  // Adjust the maximum date as needed
                validation.AllowType = CellDataType.Date;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a date between 01/01/1900 and 12/12/2023 in this cell.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid date";
            }
            else if (dataType.Equals("boolean", StringComparison.OrdinalIgnoreCase))
            {
                // Data validation formula for "TRUE" or "FALSE"
                validation.Values = new string[] { truevalue, falsevalue };
                validation.ErrorTitle = "Error";
                validation.InputTitle = "Input Data";
                validation.ErrorMessage = "Select values from dropdown";
                validation.InputMessage = "Select values from dropdown";
            }
            else if (dataType.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                // Date and time validation
                validation.CompareOperator = ValidationComparisonOperator.Between;
                validation.Formula1 = "01/01/1900";
                validation.Formula2 = "12/31/9999"; // Adjust the range as needed
                validation.AllowType = CellDataType.Date;
                validation.InputTitle = "Input Data";
                validation.InputMessage = "Type a date and time in the specified format.";
                validation.ErrorTitle = "Error";
                validation.ErrorMessage = "Enter a valid date and time.";
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


            // Add more conditions for other data types as needed
        }
        for (int i = 2; i <= 65537; i++)
        {
            string startindex = letter + i.ToString();
            string endindex = lastletter + i.ToString();
            CellRange lockrange = columnNamesWorksheet.Range[startindex + ":" + endindex];
            lockrange.Style.Locked = false;
        }
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
                    var firstCell = worksheet.Cells[1, col];
                    if (string.IsNullOrWhiteSpace(firstCell.Text))
                    {
                        // Skip this column
                        continue;
                    }

                    dataTable.Columns.Add(firstCell.Text);
                }

                for (int rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                {
                    var dataRow = dataTable.NewRow();
                    int colIndex = 0;
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        // Check if this column should be included
                        if (dataTable.Columns.Contains(worksheet.Cells[1, col].Text))
                        {
                            dataRow[colIndex] = worksheet.Cells[rowNumber, col].Text;
                            colIndex++;
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }

                dataTable = dataTable.AsEnumerable().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field.ToString()))).CopyToDataTable();
                return dataTable;
            }
        }
    }

    public List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream, int rowCount)
    {
        using (var package = new ExcelPackage(excelFileStream))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
            rowCount = rowCount + 1;
            int colCount = worksheet.Dimension.Columns;

            var data = new List<Dictionary<string, string>>();

            // Extract column names and identify which columns to skip
            var columnNames = new List<string>();
            var skipColumns = new List<bool>();
            for (int col = 1; col <= colCount; col++)
            {
                var columnName = worksheet.Cells[1, col].Value?.ToString();
                columnNames.Add(columnName);

                // Check if the first cell in this column is empty or null
                skipColumns.Add(string.IsNullOrWhiteSpace(columnName));
            }

            // Read data rows
            for (int row = 2; row <= rowCount; row++)
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

    public async Task<LogDTO> Createlog(string tableName, List<string> filedata, string fileName, int successdata, string errorMessage, string successMessage)
    {
        var storeentity = await _context.EntityListMetadataModels.FirstOrDefaultAsync(x => x.EntityName.ToLower() == tableName.ToLower());
        LogParent logParent = new LogParent();
        logParent.FileName = fileName;
        logParent.User_Id = 1;
        logParent.Entity_Id = storeentity.Id;
        logParent.Timestamp = DateTime.UtcNow;
        logParent.FailCount = filedata.Count - 1;
        logParent.PassCount = successdata;
        logParent.RecordCount = logParent.FailCount + logParent.PassCount;

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

        LogChild logChild = new LogChild();


        int parentId = logParent.ID;
        logChild.ParentID = parentId; // Set the ParentId
        if (filedata.Count() > 0)
        {
            string delimiter = ";"; // Specify the delimiter you want
            string result = string.Join(delimiter, filedata);
            logChild.Filedata = result; // Set the values as needed
            logChild.ErrorMessage = errorMessage;
        }
        else
        {
            logChild.Filedata = "";
            logChild.ErrorMessage = successMessage;
        }


        // Insert the LogChild record
        await _context.logChilds.AddAsync(logChild);
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
            ChildrenDTOs = _context.logChilds.Where(x => x.ParentID == logParent.ID).ToList()
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

        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json"); // Make sure the file path is correct

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
                var successdata = convertedDataList.Count - errorDataList.Count;
                string errorMessages = "Server error";
                string successMessage = " ";
                string fileName = file.FileName;
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
                var result = Createlog(tableName, badRows, fileName, successdata, errorMessages, successMessage);

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
            throw new Exception("Entity not found");
        }
    }

    public List<EntityListMetadataModel> GetEntityListMetadataModelforlist()
    {
        {
            
            List<EntityListMetadataModel> entityListMetadataModels = _context.EntityListMetadataModels.ToList();
            return entityListMetadataModels;
        }
    }

    public int? GetEntityIdFromTemplate(IFormFile file)
    {

        using (var package = new ExcelPackage(file.OpenReadStream()))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Assuming entity ID is in the first sheet
            int entityId;

            if (int.TryParse(worksheet.Cells[1, 1].Text, out entityId))
            {
                return entityId;
            }

            return null; // Unable to parse the entity ID from the template
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
        //if (string.IsNullOrEmpty(primaryKeyColumn))
        //{

        //    return new List<int>();
        //}
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


