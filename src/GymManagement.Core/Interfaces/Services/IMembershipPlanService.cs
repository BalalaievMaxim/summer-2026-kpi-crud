// Файл: src/GymManagement.Core/Interfaces/IMembershipPlanService.cs

using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IMembershipPlanService
{
    Task CreatePlanAsync(CreateMembershipPlanDto dto);
    Task DeleteUnusedPlanAsync(int planId);
    Task<List<MembershipPlan>> GetPlansAsync(decimal? minPrice, decimal? maxPrice);
}