using GymManagement.Core.Entities;
using GymManagement.Core.Interfaces;
using GymManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GymManagement.Infrastructure.Repositories;

public class ClassRepository(GymManagementContext context) : IClassRepository
{
    public async Task<Class?> GetByIdAsync(int classId)
    {
        return await context.Classes.FindAsync(classId);
    }
}
