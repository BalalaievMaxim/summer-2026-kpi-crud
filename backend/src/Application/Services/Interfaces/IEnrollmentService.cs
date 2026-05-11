using GymManagement.Application.DTOs;

namespace GymManagement.Application.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentResultDto> CreateEnrollmentAsync(CreateEnrollmentDto dto);
}
