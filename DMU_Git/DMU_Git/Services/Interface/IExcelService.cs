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
        List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream);
        public DataTable ReadExcelFromFormFile(IFormFile excelFile);
        public bool IsValidDataType(string data, string expectedDataType);
        public IEnumerable<EntityColumnDTO> GetColumnsForEntity(string entityName);
        public Task<LogDTO> Createlog(string tableName, List<string> filedata, string fileName, DataTable successdata);
    }
}
