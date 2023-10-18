using DMU_Git.Models.DTO;
using DMU_Git.Models;
using System.Collections.Generic;
using System.Data;

namespace DMU_Git.Services.Interface
{
    public interface IExcelService
    {

     
        byte[] GenerateExcelFile(List<EntityColumnDTO> columns);
        //important
        //List<MyDataModelDto> ReadDataFromExcel(Stream excelFileStream);
        List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream,int rowcount);
        public DataTable ReadExcelFromFormFile(IFormFile excelFile);
        public bool IsValidDataType(string data, string expectedDataType);
        public IEnumerable<EntityColumnDTO> GetColumnsForEntity(string entityName);
        Task<LogDTO> Createlog(string tableName, List<string> filedata, string fileName, DataTable successdata, string errorMessage,string successMessage);
        //int GetEntityIdByEntityName(string entityName);

        public void InsertDataFromDataTableToPostgreSQL(DataTable data, string tableName, List<string> columns);

        public int GetEntityIdByEntityNamefromui(string entityName);

        public List<EntityListMetadataModel> GetEntityListMetadataModelforlist();

        public int? GetEntityIdFromTemplate(IFormFile file);
    }
}
