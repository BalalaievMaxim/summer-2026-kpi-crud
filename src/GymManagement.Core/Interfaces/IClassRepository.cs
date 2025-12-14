using GymManagement.Core.Entities;
using System.Threading.Tasks;

namespace GymManagement.Core.Interfaces;

public interface IClassRepository
{
    Task<Class?> GetByIdAsync(int classId);
}
