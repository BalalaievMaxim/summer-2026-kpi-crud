using GymManagement.Domain.Ports;
using GymManagement.Domain.Enrollments;
using GymManagement.Infrastructure.Persistence;
using E = GymManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public sealed class EnrollmentRepository(GymManagementContext context) : IEnrollmentRepositoryPort
{
    public async Task<int> AddAsync(Enrollment enrollment,
        CancellationToken cancellationToken = default)
    {
        var entity = new E.Enrollment
        {
            ClientId = enrollment.ClientId,
            ClassId = enrollment.ClassId,
            RegistrationTime = enrollment.RegistrationTime.UtcDateTime
        };

        await context.Enrollments.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return entity.EnrollmentId;
    }

    public Task<bool> IsClientEnrolledAsync(int clientId, int classId, CancellationToken cancellationToken = default)
        => context.Enrollments.AnyAsync(
            e => e.ClientId == clientId && e.ClassId == classId,
            cancellationToken);
}
