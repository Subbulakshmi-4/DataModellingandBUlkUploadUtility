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
        public int StringMaxLength { set; get; }

        public int StringMinLength { set; get; }
        public int? NumberMaxValue { set; get; }
        public int? NumberMinValue { set; get; }
        public DateTime? DateMinValue { set; get; }

        public DateTime? DateMaxValue { set; get; }

        public string Description { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public string True { get; set; }
        public string False { get; set; }
        public bool ColumnPrimaryKey { get; set; }
    }
}
