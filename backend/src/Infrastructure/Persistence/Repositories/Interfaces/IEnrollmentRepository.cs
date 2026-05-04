using GymManagement.Infrastructure.Persistence.Entities;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Persistence.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment enrollment);
}
