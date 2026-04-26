using GymManagement.Models;
using GymManagement.Repositories.Interfaces;
using GymManagement.Configuration;
using System.Threading.Tasks;

namespace GymManagement.Repositories;

public class EnrollmentRepository(GymManagementContext context) : IEnrollmentRepository
{
    public async Task AddAsync(Enrollment enrollment)
    {
        await context.Enrollments.AddAsync(enrollment);
    }
}
