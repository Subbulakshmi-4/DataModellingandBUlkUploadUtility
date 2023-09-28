namespace DMU_Git.Models.DTO
{
    public class EntityColumnDTO
    {
        public int Id { get; set; }
        public string EntityColumnName { get; set; }
        public string Datatype { get; set; }
        public int Length { get; set; }
        public bool IsNullable { get; set; }
        public string DefaultValue { get; set; }
        public bool ColumnPrimaryKey { get; set; }

        public static explicit operator EntityColumnDTO(EntityColumnListMetadataModel data)
        {
            return new EntityColumnDTO
            {
                Id = data.Id,
                EntityColumnName = data.EntityColumnName,
                Datatype = data.Datatype,
                Length = data.Length,
                IsNullable = data.IsNullable,
                DefaultValue = data.DefaultValue,
                ColumnPrimaryKey = data.ColumnPrimaryKey
            };
        }

        public static implicit operator EntityColumnListMetadataModel(EntityColumnDTO data)
        {
            return new EntityColumnListMetadataModel
            {
                Id = data.Id,
                EntityColumnName = data.EntityColumnName,
                Datatype = data.Datatype,
                Length = data.Length,
                IsNullable = data.IsNullable,
                DefaultValue = data.DefaultValue,
                ColumnPrimaryKey = data.ColumnPrimaryKey
            };
        }
    }
}
