using GymManagement.Infrastructure.Persistence.Entities;
using GymManagement.Infrastructure.Persistence.Repositories.Interfaces;
using GymManagement.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository(GymManagementContext context) : IEnrollmentRepository
{
    public async Task AddAsync(Enrollment enrollment)
    {
        await context.Enrollments.AddAsync(enrollment);
    }
}
