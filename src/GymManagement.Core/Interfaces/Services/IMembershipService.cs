using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;
using GymManagement.Core.Enums;

namespace GymManagement.Core.Interfaces;

public interface IMembershipService
{
    Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes);
}