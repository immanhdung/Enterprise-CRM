using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Accounts;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    [HasPermission(Permissions.CustomersView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] string? search,
        [FromQuery] string? classification,
        [FromQuery] string? industry,
        [FromQuery] string? status,
        [FromQuery] Guid? assignedEmployeeId,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _accountService.GetAccountsAsync(
            search, classification, industry, status, assignedEmployeeId,
            sortBy, sortDescending, pageIndex, pageSize, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CustomersView)]
    [ProducesResponseType(typeof(ApiResponse<AccountDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccountDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetailById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.GetAccountDetailByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.CustomersCreate)]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.CreateAccountAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetAccountDetailById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CustomersEdit)]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.UpdateAccountAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CustomersDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.DeleteAccountAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}
