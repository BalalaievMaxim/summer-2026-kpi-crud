using GymManagement.DTOs;
using GymManagement.Models;

namespace GymManagement.Repositories.Interfaces;

public interface IEnrollmentService
{
    Task<Enrollment> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto);
}