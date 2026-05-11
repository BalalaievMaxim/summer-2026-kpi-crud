using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class EnrollmentService(
    IEnrollmentRepositoryPort enrollmentRepository,
    IClientRepository clientRepository,
    IClassScheduleRepository classRepository,
    IMembershipRepositoryPort membershipRepository,
    EnrollmentFactory enrollmentFactory) : IEnrollmentService
{
    public async Task<EnrollmentResultDto> CreateEnrollmentAsync(CreateEnrollmentDto dto)
    {
        if (!await clientRepository.ExistsAsync(dto.ClientId))
            throw new InvalidOperationException("Client not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(dto.ClientId);
        var hasActiveMembership = activeMemberships.Any(m =>
            m.IsActive &&
            m.StartDate <= today &&
            m.EndDate >= today);

        if (!hasActiveMembership)
            throw new InvalidOperationException("Client does not have an active membership.");

        var enrollment = await enrollmentFactory.CreateAsync(dto.ClientId, dto.ClassId);

        var enrollmentId = await enrollmentRepository.AddAsync(dto.ClientId, dto.ClassId, DateTime.UtcNow);

        return new EnrollmentResultDto(enrollmentId, dto.ClientId, dto.ClassId);
    }
}
