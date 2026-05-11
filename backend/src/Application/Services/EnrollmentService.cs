using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class EnrollmentService(
    IEnrollmentRepositoryPort enrollmentRepository,
    IClientRepository clientRepository,
    IClassScheduleRepository classRepository,
    IMembershipRepositoryPort membershipRepository) : IEnrollmentService
{
    public async Task<EnrollmentResultDto> CreateEnrollmentAsync(CreateEnrollmentDto dto)
    {
        if (!await clientRepository.ExistsAsync(dto.ClientId))
            throw new InvalidOperationException("Client not found.");

        var session = await classRepository.GetByIdWithEnrollmentsAsync(dto.ClassId);
        if (session is null)
            throw new InvalidOperationException("Class not found.");

        if (session.EnrollmentClientIds.Contains(dto.ClientId))
            throw new InvalidOperationException("Client is already enrolled in this class.");

        if (session.EnrollmentClientIds.Count >= session.Capacity)
            throw new InvalidOperationException("Class is full.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(dto.ClientId);
        var hasActiveMembership = activeMemberships.Any(m =>
            m.IsActive &&
            m.StartDate <= today &&
            m.EndDate >= today);

        if (!hasActiveMembership)
            throw new InvalidOperationException("Client does not have an active membership.");

        var enrollmentId = await enrollmentRepository.AddAsync(dto.ClientId, dto.ClassId, DateTime.UtcNow);

        return new EnrollmentResultDto(enrollmentId, dto.ClientId, dto.ClassId);
    }
}
