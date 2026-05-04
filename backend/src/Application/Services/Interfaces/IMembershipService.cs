using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Entities.Enums;

namespace GymManagement.Application.Services.Interfaces;

public interface IMembershipService
{
    Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes);
}