using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Clients;
using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

namespace GymManagement.Application.Services;

public class EnrollmentService(
    IEnrollmentRepository enrollmentRepository,
    IClientRepository clientRepository,
    IClassRepository classRepository,
    IMembershipRepository membershipRepository,
    IUnitOfWork unitOfWork) : IEnrollmentService
{
    public async Task<Enrollment> CreateEnrollmentAsync(CreateEnrollmentDto dto)
    {
        if (!await clientRepository.ExistsAsync(dto.ClientId))
            throw new InvalidOperationException("Client not found.");

        var @class = await classRepository.GetByIdWithEnrollmentsAsync(dto.ClassId);
        if (@class == null)
            throw new InvalidOperationException("Class not found.");

        if (@class.Enrollments.Any(e => e.ClientId == dto.ClientId))
            throw new InvalidOperationException("Client is already enrolled in this class.");

        if (@class.Enrollments.Count >= @class.Capacity)
            throw new InvalidOperationException("Class is full.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeMemberships = await membershipRepository.GetActiveMembershipsByClientAsync(dto.ClientId);
        var hasActiveMembership = activeMemberships.Any(m =>
            m.IsActive == true &&
            m.StartDate <= today &&
            m.EndDate >= today);

        if (!hasActiveMembership)
            throw new InvalidOperationException("Client does not have an active membership.");

        var enrollment = new Enrollment
        {
            ClientId = dto.ClientId,
            ClassId = dto.ClassId,
            RegistrationTime = DateTime.UtcNow
        };

        await enrollmentRepository.AddAsync(enrollment);
        await unitOfWork.SaveChangesAsync();

        return enrollment;
    }
}
