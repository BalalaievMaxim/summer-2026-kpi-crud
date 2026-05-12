namespace GymManagement.Application.DTOs;

public sealed record ClientActivityRow(
    int ClientId,
    string Name,
    string Email,
    int TotalEnrollments,
    int ClientRank);

public sealed record ClassAttendanceRow(
    int ClassId,
    string ClassName,
    string CoachName,
    DateTime StartTime,
    int Capacity,
    int CurrentEnrollment,
    decimal OccupancyRate);

public sealed record CoachWorkloadRow(
    int CoachId,
    string CoachName,
    int TotalClassesScheduled,
    int TotalHoursWorked,
    int AverageClassSize);

public sealed record CoachEfficiencyRow(
    int CoachId,
    string CoachName,
    string Specialization,
    int TotalHours,
    int ClassCount,
    decimal AverageOccupancyPercent,
    int CoachRank);

public sealed record TotalMembershipRevenueRow(
    string RevenueMonth,
    string PlanName,
    decimal TotalRevenue);
