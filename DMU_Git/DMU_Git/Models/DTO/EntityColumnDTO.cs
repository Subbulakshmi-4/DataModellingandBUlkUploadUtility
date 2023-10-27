using System.ComponentModel.DataAnnotations.Schema;

namespace DMU_Git.Models.DTO
{
    public class EntityColumnDTO
    {
        public int Id { get; set; }
        public string EntityColumnName { get; set; }
        public string entityname { get; set; }

        [ForeignKey("EntityId")]
        public int EntityId { get; set; }
        public string Datatype { get; set; }
        public int Length { get; set; }
        public int? MinLength { set; get; }
        public int? MaxLength { set; get; }
        public string DateMinValue { set; get; }
        public string DateMaxValue { set; get; }
        public string Description { get; set; } // New property
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public bool ColumnPrimaryKey { get; set; }

        public string True { get; set; }

        public string False { get; set; }

        public static explicit operator EntityColumnDTO(EntityColumnListMetadataModel data)
        {
            return new EntityColumnDTO
            {
                Id = data.Id,
                EntityColumnName = data.EntityColumnName,
                EntityId = data.EntityId,
                Datatype = data.Datatype,
                Length = data.Length,
                MinLength = data.MinLength,
                MaxLength = data.MaxLength,
                DateMinValue = data.DateMinValue,
                DateMaxValue = data.DateMaxValue,
                Description = data.Description, // Map the Description property
                IsNullable = data.IsNullable,
                DefaultValue = data.DefaultValue,
                ColumnPrimaryKey = data.ColumnPrimaryKey,
                True = data.True,
                False = data.False,
            };
        }

        public static implicit operator EntityColumnListMetadataModel(EntityColumnDTO data)
        {
            return new EntityColumnListMetadataModel
            {
                Id = data.Id,
                EntityColumnName = data.EntityColumnName,
                EntityId = data.EntityId,
                Datatype = data.Datatype,
                Length = data.Length,
                MinLength = data.MinLength,
                MaxLength = data.MaxLength,
                DateMinValue = data.DateMinValue,
                DateMaxValue = data.DateMaxValue,
                Description = data.Description, // Map the Description property
                IsNullable = data.IsNullable,
                DefaultValue = data.DefaultValue,
                ColumnPrimaryKey = data.ColumnPrimaryKey,
                True = data.True,
                False= data.False,
            };
        }
    }
}
