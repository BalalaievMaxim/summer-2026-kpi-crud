using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class EnrollmentRepository(GymManagementContext context) : IEnrollmentRepository
{
    public async Task AddAsync(Enrollment enrollment)
    {
        await context.Enrollments.AddAsync(enrollment);
    }
}
