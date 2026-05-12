using GymManagement.Application.DTOs;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Features.Invoices.Commands.CreateInvoice;
using GymManagement.Application.Features.Invoices.Commands.MarkInvoicePaid;
using GymManagement.Application.Features.Invoices.Queries.GetMonthlyRevenueByPlan;
using GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;
using GymManagement.Domain.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Presentation.Controllers;

[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public sealed class InvoiceController : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(InvoiceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInvoiceAsync(
        [FromBody] CreateInvoiceRequestDto requestDto,
        [FromServices] ICommandHandler<CreateInvoiceCommand, int> commandHandler,
        CancellationToken cancellationToken)
    {
        var invoiceId = await commandHandler.Handle(
            new CreateInvoiceCommand(
                requestDto.ClientId,
                requestDto.PaymentMethod,
                requestDto.MembershipPlanId,
                requestDto.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetPendingInvoices), new { clientId = requestDto.ClientId }, new { invoiceId });
    }

    [HttpPut("{invoiceId}/pay")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkInvoiceAsPaid(
        int invoiceId,
        [FromServices] ICommandHandler<MarkInvoicePaidCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        await commandHandler.Handle(new MarkInvoicePaidCommand(invoiceId), cancellationToken);
        return NoContent();
    }

    [HttpGet("pending/{clientId}")]
    [ProducesResponseType(typeof(List<InvoiceResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingInvoices(
        int clientId,
        [FromServices] IQueryHandler<GetPendingInvoicesForClientQuery, List<InvoiceRecord>> queryHandler,
        CancellationToken cancellationToken)
    {
        var invoices = await queryHandler.Handle(new GetPendingInvoicesForClientQuery(clientId), cancellationToken);

        var response = invoices.Select(MapToResponseDto).ToList();

        return Ok(response);
    }

    [HttpGet("analytics/revenue-by-plan")]
    public async Task<ActionResult<List<TotalMembershipRevenueRow>>> GetRevenueAnalytics(
        [FromServices] IQueryHandler<GetMonthlyRevenueByPlanQuery, List<TotalMembershipRevenueRow>> queryHandler,
        CancellationToken cancellationToken)
    {
        var analytics = await queryHandler.Handle(new GetMonthlyRevenueByPlanQuery(), cancellationToken);
        return Ok(analytics);
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
