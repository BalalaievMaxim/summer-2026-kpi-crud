using GymManagement.Domain.Billing;
using GymManagement.Domain.Memberships;
namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipService
{
    Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes);
    Task<IReadOnlyList<MembershipRecord>> GetActiveMembershipsByClientAsync(int clientId);
}
