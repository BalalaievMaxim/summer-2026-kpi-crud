using GymManagement.Models;
using System.Threading.Tasks;

namespace GymManagement.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment enrollment);
}
