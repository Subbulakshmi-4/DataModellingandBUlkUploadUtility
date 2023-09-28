using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DMU_Git.Controllers
{
    [Route("api/entitylist")]
    [ApiController]
    public class EntitylistDisplayController : Controller
    {

        private readonly IEntitylistService _entitylistService;

        public EntitylistDisplayController(IEntitylistService entitylistService)
        {
            _entitylistService = entitylistService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EntityListDto>> Get()
        {
            var tablename = _entitylistService.GetEntityList();
            return Ok(tablename);
        }



    }
}
