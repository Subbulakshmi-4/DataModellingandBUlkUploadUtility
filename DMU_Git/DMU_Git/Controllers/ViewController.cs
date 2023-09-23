using DMU_Git.Services;
using Microsoft.AspNetCore.Mvc;

namespace DMU_Git.Controllers
{
    [Route("api/entity")]
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
            var columns = _viewService.GetColumnsForEntity(entityName);

            if (columns == null)
            {
                return NotFound();
            }

            return Ok(columns);
        }
    }
}
