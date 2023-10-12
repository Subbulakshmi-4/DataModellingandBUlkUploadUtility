using DMU_Git.Models;
using DMU_Git.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using DMU_Git.Models.TableCreationRequestDTO;
using System.Net;



namespace DMU_Git.Controllers
{
    [Route("api/dynamic")]
    [EnableCors("AllowAngularDev")]
    [ApiController]
    public class DynamicDbController : ControllerBase
    {
        private readonly DynamicDbService _dynamicDbService;



        public DynamicDbController(DynamicDbService dynamicDbService)
        {
            _dynamicDbService = dynamicDbService;
        }




        [HttpPost("create-table")]
        public async Task<ActionResult> CreateTable([FromBody] TableCreationRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessage = new List<string> { "Invalid request data." },
                        Result = null
                    };



                    return BadRequest(response);
                }

                var existingTable = await _dynamicDbService.TableExistsAsync(request.TableName);
                if (existingTable)
                {
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        ErrorMessage = new List<string> { $"Table '{request.TableName}' already exists." },
                        Result = null
                    };

                    return BadRequest(response);
                }

                bool tableCreated = await _dynamicDbService.CreateDynamicTableAsync(MapToModel(request));



                if (tableCreated)
                {
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.OK,
                        IsSuccess = true,
                        ErrorMessage = new List<string>(),
                        Result = $"Table '{request.TableName}' created successfully."
                    };



                    return Ok(response);
                }
                else
                {
                    var response = new APIResponse
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        IsSuccess = false,
                        ErrorMessage = new List<string> { $"An error occurred while creating the table '{request.TableName}'." },
                        Result = null
                    };



                    return StatusCode((int)HttpStatusCode.InternalServerError, response);
                }
            }
            catch (Exception ex)
            {
                var response = new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessage = new List<string> { "An error occurred while creating the table." },
                    Result = null
                };



                Console.WriteLine(ex);
                return StatusCode((int)HttpStatusCode.InternalServerError, response);
            }
        }



        // method to map the DTO to the original model
        private TableCreationRequest MapToModel(TableCreationRequestDTO dto)
        {
            return new TableCreationRequest
            {
                TableName = dto.TableName,
                Columns = dto.Columns.Select(columnDto => new ColumnDefinition
                {
                    EntityColumnName = columnDto.EntityColumnName,
                    DataType = columnDto.DataType,
                    Length = columnDto.Length,
                    Description = columnDto.Description,
                    IsNullable = columnDto.IsNullable,
                    DefaultValue = columnDto.DefaultValue,
                    ColumnPrimaryKey = columnDto.ColumnPrimaryKey
                }).ToList()
            };
        }
    }
}