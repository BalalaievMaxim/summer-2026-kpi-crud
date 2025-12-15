using GymManagement.Core.Entities;
using System.Threading.Tasks;

namespace GymManagement.Core.Interfaces;

public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment enrollment);
}
