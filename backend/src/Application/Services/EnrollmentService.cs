using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Clients.Errors;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Enrollments.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public sealed class EnrollmentService(
    IEnrollmentRepositoryPort enrollmentRepository,
    IClientRepository clientRepository,
    IMembershipRepositoryPort membershipRepository,
    EnrollmentFactory enrollmentFactory) : IEnrollmentService
{
    public async Task<EnrollmentResultDto> CreateEnrollmentAsync(CreateEnrollmentDto dto)
    {
        if (!await clientRepository.ExistsAsync(dto.ClientId))
            throw new ClientNotFoundError(dto.ClientId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(dto.ClientId);
        var hasActiveMembership = activeMemberships.Any(m => m.IsCurrentlyActive(today));

        if (!hasActiveMembership)
            throw new ClientHasNoActiveMembershipError(dto.ClientId);

        var enrollment = await enrollmentFactory.CreateAsync(dto.ClientId, dto.ClassId, DateTimeOffset.UtcNow);

        var enrollmentId = await enrollmentRepository.AddAsync(enrollment);

        return new EnrollmentResultDto(enrollmentId, dto.ClientId, dto.ClassId);
    }
}
