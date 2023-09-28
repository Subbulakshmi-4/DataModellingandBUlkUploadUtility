using DMU_Git.Models.DTO;
using DMU_Git.Models;
using System.Collections.Generic;

namespace DMU_Git.Services.Interface
{
    public interface IExcelService
    {

        byte[] GenerateExcelFile(List<TableColumn> columns);
        //important
        //List<MyDataModelDto> ReadDataFromExcel(Stream excelFileStream);
        List<Dictionary<string, string>> ReadDataFromExcel(Stream excelFileStream);
    }
}
