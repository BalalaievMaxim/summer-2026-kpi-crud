using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GymManagement.Application.Services;
using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Exceptions;
using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

[ApiController]
[Route("api/v1/invoices")]
public class InvoiceController(IInvoiceService service) : ControllerBase
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
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
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
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
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
    public async Task<ActionResult<List<TotalMembershipRevenueDto>>> GetRevenueAnalytics()
    {
        try
        {
            var analytics = await service.GetMonthlyRevenueAnalyticsAsync();
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private static InvoiceResponseDto MapToResponseDto(Invoice invoice)
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