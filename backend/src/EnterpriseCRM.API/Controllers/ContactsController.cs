using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.Contacts;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/contacts")]
[Authorize]
public class ContactsController : BaseApiController
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    [HasPermission(Permissions.CustomersView)]
    [ProducesResponseType(typeof(ApiResponse<PaginatedList<ContactDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(
        [FromQuery] string? search,
        [FromQuery] Guid? accountId,
        [FromQuery] string? status,
        [FromQuery] bool? isPrimary,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _contactService.GetContactsAsync(search, accountId, status, isPrimary, pageIndex, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.CustomersView)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.GetContactByIdAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost]
    [HasPermission(Permissions.CustomersCreate)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContact([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var result = await _contactService.CreateContactAsync(request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return CreatedAtAction(nameof(GetContactById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.CustomersEdit)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ContactDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContact(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var result = await _contactService.UpdateContactAsync(id, request, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.CustomersDelete)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.DeleteContactAsync(id, cancellationToken);
        if (!result.Success) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }
}
