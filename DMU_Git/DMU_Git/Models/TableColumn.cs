namespace DMU_Git.Models
{
    public class TableColumn
    {
        public int Id { get; set; }
        public string EntityColumnName { get; set; }
        public string Datatype { get; set; }
        public int Length { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public bool ColumnPrimaryKey { get; set; }
    }
}
