using DMU_Git.Data;
using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services.Interface;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DMU_Git.Controllers
{
    [Route("api/entitylist")]
    [EnableCors("AllowAngularDev")]
    [ApiController]
    public class EntitylistDisplayController : Controller
    {

        private readonly IEntitylistService _entitylistService;
        protected APIResponse _response;


        public EntitylistDisplayController(IEntitylistService entitylistService)
        {
            _entitylistService = entitylistService;
            _response = new();
        }

        [HttpGet]
        [ProducesResponseType(200)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<EntityListDto>> Get()
        {
            
            var tablename = _entitylistService.GetEntityList();
            if(tablename == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessage.Add("No Data Available");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tablename;
            return Ok(_response);
        }



    }
}
