﻿namespace DMU_Git.Models.TableCreationRequestDTO
{
    public class TableCreationRequestDTO
    {
        public string TableName { get; set; }
        public List<ColumnDefinitionDTO> Columns { get; set; }
    }
    public class ColumnDefinitionDTO
    {
        public string EntityColumnName { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public int? MinLength { set; get; }
        public int? MaxLength { set; get; }
        public int? MaxRange { set; get; }
        public int? MinRange { set; get; }
        public string DateMinValue { set; get; }
        public string DateMaxValue { set; get; }
        public string Description { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public string True { get; set; }
        public string False { get; set; }
        public bool ColumnPrimaryKey { get; set; }
    }
}
