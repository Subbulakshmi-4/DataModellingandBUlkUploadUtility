using DMU_Git.Models;
using DMU_Git.Models.DTO;
using DMU_Git.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Collections.Generic;

namespace DMU_Git.Controllers
{
    [Route("api/entity")]
    [EnableCors("AllowAngularDev")]
    [ApiController]
    public class ViewController : ControllerBase
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
                return NotFound(new APIResponse
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessage = new List<string> { "Table not found" },
                    Result = null
                });
            }
            return Ok(new APIResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Result = columnsDTO
            });
        }

        //[Route("api/setconnectionstring")]
        //[HttpPost]
        //public IActionResult SetConnectionString([FromBody] ConnectionStringDTO model)
        //{
        //    // Process and use the connection string received in the model
        //    string connectionString = model.ConnectionString;

        //    // Perform actions with the connection string (e.g., database operations)

        //    return Ok(new { message = "Connection string set successfully" });
        //}
    }
}
