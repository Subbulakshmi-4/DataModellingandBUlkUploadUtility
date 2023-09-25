using DMU_Git.Models.DTO;
using DMU_Git.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace DMU_Git.Controllers
{
    [Route("api/entity")]
    [EnableCors("AllowAngular")]
    [ApiController]
    public class ViewController : Controller
    {
        private readonly ViewService _viewService;

        public ViewController(ViewService viewService)
        {
            _viewService = viewService;
        }

        [HttpGet("{entityName}/columns")]
        public IActionResult GetColumnsForEntity(string entityName)
        {
            var columnsDTO = _viewService.GetColumnsForEntity(entityName);

            if (columnsDTO == null)
            {
                var response = new APIResponse<List<EntityColumnDTO>>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessage = new List<string> { "Columns not found" },
                    Result = null
                };
                return NotFound(response);
            }

            var apiResponse = new APIResponse<List<EntityColumnDTO>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                ErrorMessage = null,
                Result = (List<EntityColumnDTO>)columnsDTO
            };

            return Ok(apiResponse);
        }

    }
}
