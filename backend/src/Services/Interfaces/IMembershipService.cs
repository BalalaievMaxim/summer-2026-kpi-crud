using GymManagement.DTOs;
using GymManagement.Models;
using GymManagement.Models.Enums;

namespace GymManagement.Repositories.Interfaces;

public interface IMembershipService
{
    Task PurchaseMembershipAsync(int clientId, int planId, PaymentMethod method, string? notes);
}