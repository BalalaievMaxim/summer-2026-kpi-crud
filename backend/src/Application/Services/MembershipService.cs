using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Billing;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Memberships;
using GymManagement.Domain.Memberships.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class MembershipService(
    IMembershipRepositoryPort membershipRepository,
    IClientRepository clientRepository,
    IInvoiceService invoiceService,
    IMembershipPlanRepositoryPort membershipPlanRepository,
    IUnitOfWork unitOfWork
) : IMembershipService
{
    public async Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes)
    {
        if (!await clientRepository.ExistsAsync(clientId))
            throw new ClientNotFoundError(clientId);

        var plan = await membershipPlanRepository.GetByIdAsync(planId);
        if (plan is null)
            throw new MembershipPlanNotFoundError(planId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (await membershipRepository.HasActiveMembershipForPlanAsync(clientId, planId, today))
            throw new ActiveMembershipExistsError(clientId, planId);

        await invoiceService.CreateInvoiceAsync(clientId, method, planId, notes);

        var membership = Membership.PurchasePending(clientId, plan, today);

        await membershipRepository.AddAsync(membership);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MembershipDto>> GetActiveMembershipsByClientAsync(int clientId)
    {
        var list = await membershipRepository.GetActiveMembershipsByClientAsync(clientId);
        return list.Select(ToDto).ToList();
    }

    private static MembershipDto ToDto(Membership membership)
        => new(
            membership.Id,
            membership.ClientId,
            membership.PlanId,
            membership.Period.Start,
            membership.Period.End,
            membership.IsActive);
}
