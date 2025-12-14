using GymManagement.Application.DTOs;

using GymManagement.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Api.Controllers;

public class InvoiceController (IInvoiceService service) : ControllerBase
{
    [HttpPost]
    [Route("/api/v1/invoices/create")]
    public async Task<IActionResult> CreateInvoiceAsync([FromBody]CreateInvoiceRequestDto requestDto)
    {
        if (requestDto == null)
        {
            return BadRequest(new { error = "Request body is required." });
        }
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var result = await service.CreateInvoiceAsync(requestDto.ClientId, requestDto.PaymentMethod, requestDto.MembershipPlanId, requestDto.Notes);

            var response = new CreateInvoiceResponseDto()
            {
                InvoiceId = result.InvoiceId,
                ClientId = result.ClientId,
                Amount = result.Amount,
                PaymentMethod = result.PaymentMethod,
                InvoiceDate= result.Date,

            };
            return Ok(response); 
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}