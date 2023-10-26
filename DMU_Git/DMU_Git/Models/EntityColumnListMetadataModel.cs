using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMU_Git.Models
{
    public class EntityColumnListMetadataModel : BaseModel
    {

        public int Id { get; set; }
        public string EntityColumnName { get; set; }

        [ForeignKey("EntityId")]
        public int EntityId { get; set; }

        public string Datatype { get; set; }

        public int Length { get; set; }
        public int StringMaxLength { set; get; }

        public int StringMinLength { set; get; }

        public int? NumberMaxValue { set; get; }
        public int? NumberMinValue { set; get; }

        public string DateMinValue { set; get; }

        public string DateMaxValue { set; get; }

        public string Description { get; set; }

        public bool IsNullable { get; set; }

        public string DefaultValue { get; set; }

        public string True { get; set; }

        public string False { get; set; }

        public bool ColumnPrimaryKey { get; set; }

        public EntityListMetadataModel EntityList { get; set; }
    }
}
