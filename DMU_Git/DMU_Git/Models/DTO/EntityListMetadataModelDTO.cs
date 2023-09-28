namespace DMU_Git.Models.DTO
{
    public class EntityListMetadataModelDTO
    {
        public int Id { get; set; }
        public string EntityName { get; set; }
        public ICollection<EntityColumnDTO> EntityColumnListMetadata { get; set; }

        public static explicit operator EntityListMetadataModelDTO(EntityListMetadataModel data)
        {
            return new EntityListMetadataModelDTO
            {
                Id = data.Id,
                EntityName = data.EntityName,
                EntityColumnListMetadata = data.EntityColumnListMetadata.Select(c => (EntityColumnDTO)c).ToList()
            };
        }

        public static implicit operator EntityListMetadataModel(EntityListMetadataModelDTO data)
        {
            return new EntityListMetadataModel
            {
                Id = data.Id,
                EntityName = data.EntityName,
                EntityColumnListMetadata = data.EntityColumnListMetadata.Select(c => (EntityColumnListMetadataModel)c).ToList()
            };
        }
    }
}
