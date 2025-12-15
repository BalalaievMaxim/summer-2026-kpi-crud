using GymManagement.Core.DTOs;
using GymManagement.Core.Entities;

namespace GymManagement.Core.Interfaces;

public interface IEnrollmentService
{
    Task<Enrollment> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
}