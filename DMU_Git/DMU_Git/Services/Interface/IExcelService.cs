using DMU_Git.Models.DTO;
using DMU_Git.Models;
using System.Data;

namespace DMU_Git.Services.Interface
{
    public interface IExcelService
    {
        byte[] GenerateExcelFile(List<EntityColumnDTO> columns, int? parentId);
        List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream,int rowcount);
        public DataTable ReadExcelFromFormFile(IFormFile excelFile);
        public bool IsValidDataType(string data, string expectedDataType);
        public IEnumerable<EntityColumnDTO> GetColumnsForEntity(string entityName);
        Task<LogDTO> Createlog(string tableName, List<string> filedata, string fileName, int successdata, string errorMessage,string successMessage);
        public void InsertDataFromDataTableToPostgreSQL(DataTable data, string tableName, List<string> columns, IFormFile file);
        public int GetEntityIdByEntityNamefromui(string entityName);
        public List<EntityListMetadataModel> GetEntityListMetadataModelforlist();
        public int? GetEntityIdFromTemplate(IFormFile file);
        public  Task<List<int>> GetAllIdsFromDynamicTable(string tableName);
        public bool TableExists(string tableName);
        public bool IsValidByteA(string data);
        public bool IsHexString(string input);
    }
}
