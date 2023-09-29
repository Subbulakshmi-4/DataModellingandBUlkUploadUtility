namespace DMU_Git.Models
{
    public class TableCreationRequest
    {
        public string TableName { get; set; }
        public List<ColumnDefinition> Columns { get; set; }
    }
         
    public class ColumnDefinition
    {
        public string EntityColumnName { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public bool ColumnPrimaryKey { get; set; }
    }
}
