using GymManagement.Application.DTOs;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Coaches.Errors;
using GymManagement.Domain.Ports;

namespace GymManagement.Application.Services;

public class CoachService(
    ICoachRepository coachRepository,
    IUnitOfWork unitOfWork) : ICoachService
{
    public Task<Coach?> GetByIdAsync(int id)
        => coachRepository.GetByIdAsync(id);

    public Task<IEnumerable<Coach>> GetAllAsync()
        => coachRepository.GetAllAsync();

    public Task<IEnumerable<Coach>> GetBySpecializationAsync(string specialization)
        => coachRepository.GetBySpecializationAsync(specialization);

    public async Task<Coach> RegisterCoachAsync(CreateCoachDto dto)
    {
        if (await coachRepository.ExistsByEmailAsync(dto.Email))
            throw new CoachEmailAlreadyExistsError(dto.Email);

        var coach = Coach.Create(dto.Name, dto.Email, dto.Specialization, dto.Password);
        var created = await coachRepository.AddAsync(coach);
        await unitOfWork.SaveChangesAsync();
        return created;
    }

    public async Task DeleteCoachAsync(int id)
    {
        if (!await coachRepository.ExistsAsync(id))
            throw new CoachNotFoundError(id);

        if (await coachRepository.HasUpcomingClassesWithEnrollmentsAsync(id))
            throw new CoachHasFutureClassesError(id);

        await coachRepository.DeleteUpcomingClassesByCoachAsync(id);
        await coachRepository.DeleteAsync(id);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateSpecializationAsync(int id, string specialization)
    {
        var coach = await coachRepository.GetByIdAsync(id)
            ?? throw new CoachNotFoundError(id);

        coach.UpdateSpecialization(specialization);

        await coachRepository.UpdateAsync(coach);
        await unitOfWork.SaveChangesAsync();
    }
}
