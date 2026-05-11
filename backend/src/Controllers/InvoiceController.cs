using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Queries;
using GymManagement.Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public sealed class InvoiceController(IInvoiceService service) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInvoiceAsync([FromBody] CreateInvoiceRequestDto requestDto)
    {
        try
        {
            var result = await service.CreateInvoiceAsync(
                requestDto.ClientId,
                requestDto.PaymentMethod,
                requestDto.MembershipPlanId,
                requestDto.Notes);

            var response = MapToResponseDto(result);

            return CreatedAtAction(nameof(GetPendingInvoices), new { clientId = result.ClientId }, response);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new { error = "Unable to create invoice." });
        }
    }

    [HttpPut("{invoiceId}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInvoiceAsPaid(int invoiceId)
    {
        try
        {
            await service.UpdatePaidInvoiceAsync(invoiceId);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return BadRequest(new { error = "Unable to update invoice status." });
        }
    }

    [HttpGet("pending/{clientId}")]
    [ProducesResponseType(typeof(List<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingInvoices(int clientId)
    {
        var invoices = await service.GetAllPendingInvoicesAsync(clientId);

        var response = invoices.Select(MapToResponseDto).ToList();

        return Ok(response);
    }

    [HttpGet("analytics/revenue-by-plan")]
    public async Task<ActionResult<List<TotalMembershipRevenueRow>>> GetRevenueAnalytics()
    {
        try
        {
            var analytics = await service.GetMonthlyRevenueAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Unable to load revenue analytics." });
        }
    }

    private static InvoiceResponseDto MapToResponseDto(InvoiceRecord invoice)
    {
        return new InvoiceResponseDto
        {
            InvoiceId = invoice.InvoiceId,
            ClientId = invoice.ClientId,
            Amount = invoice.Amount,
            PaymentMethod = invoice.PaymentMethod ?? "Unknown",
            Status = invoice.Status,
            Date = invoice.Date,
            Notes = invoice.Notes
        };
    }
}
