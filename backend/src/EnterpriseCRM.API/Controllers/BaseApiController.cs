using EnterpriseCRM.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Succeeded(data, message));
    }

    protected ActionResult<ApiResponse<T>> FailResponse<T>(string error, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, ApiResponse<T>.Failed(error));
    }

    protected ActionResult<ApiResponse<T>> FailResponse<T>(List<string> errors, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, ApiResponse<T>.Failed(errors));
    }
}
