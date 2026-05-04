using GymManagement.Infrastructure.DTOs;
using GymManagement.Application.DTOs;
using GymManagement.Infrastructure.Persistence.Entities;

namespace GymManagement.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task<Enrollment> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
}