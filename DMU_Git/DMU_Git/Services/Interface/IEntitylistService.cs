using DMU_Git.Models.DTO;

namespace DMU_Git.Services.Interface
{
    public interface IEntitylistService
    {
        IEnumerable<EntityListDto> GetEntityList();
    }
}
