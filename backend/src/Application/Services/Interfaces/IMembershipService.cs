using GymManagement.Application.DTOs;
using GymManagement.Domain.Billing;
namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipService
{
    Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes);
    Task<IReadOnlyList<MembershipDto>> GetActiveMembershipsByClientAsync(int clientId);
}
