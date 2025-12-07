using System.Net.Http.Headers;
using GymManagement.Application.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Enums;
using GymManagement.Infrastructure.Repositories;

namespace GymManagement.Application.Services;

public class MembershipService (
    MembershipRepository membershipRepository,
    ClientRepository clientRepository,
    InvoiceService invoiceService,
    MembershipPlanRepository membershipPlanRepository,
    InvoiceRepository invoiceRepository,
    UnitOfWork unitOfWork
    )
{
    public async Task CreatePlanAsync(CreateMembershipPlanDto dto)
    {
        
        var plan = new Membershipplan()
        {
            Name = dto.Name,
            DurationMonths = dto.DurationMonth,
            Price = dto.Price
        };

        if (plan.DurationMonths > 0 && plan.Name != String.Empty && plan.Price > 0)
        {
            await membershipPlanRepository.AddAsync(plan);
        }
    }

    public async Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes)
    {
        Membershipplan plan = await membershipRepository.GetMembershipPlanByIdAsync(planId);
        Membership membership = new Membership();
        
        membership.PlanId = planId;
        membership.ClientId = clientId;
        membership.StartDate  = DateOnly.FromDateTime(DateTime.Now);
        membership.EndDate = membership.StartDate.AddMonths(plan.DurationMonths);
        membership.IsActive = false;
        membership.Plan = plan;
        
        var client = await clientRepository.GetClientByIdAsync(clientId);
        if ( client != null)
        {
            membership.Client = client;
        }

        Invoice invoice = await invoiceService.CreateInvoiceAsync(clientId, method, planId, notes);
        
        await membershipRepository.AddAsync(membership);
        await invoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

    }
}