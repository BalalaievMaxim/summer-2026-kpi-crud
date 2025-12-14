using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class ClassRepository(GymManagementContext context) : IClassRepository
{
    public async Task<Class?> GetByIdAsync(int classId)
    {
        return await context.Classes.FindAsync(classId);
    }

    public async Task<Class?> GetByIdWithEnrollmentsAsync(int classId)
    {
        return await context.Classes
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }
}
