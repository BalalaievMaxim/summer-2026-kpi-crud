using GymManagement.Models;
using GymManagement.Repositories.Interfaces;
using GymManagement.DTOs;

namespace GymManagement.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IClassRepository _classRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository, 
        IClientRepository clientRepository, 
        IClassRepository classRepository, 
        IUnitOfWork unitOfWork)
    {
        _enrollmentRepository = enrollmentRepository;
        _clientRepository = clientRepository;
        _classRepository = classRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Enrollment> CreateEnrollmentAsync(CreateEnrollmentDto createEnrollmentDto)
    {
        var client = await _clientRepository.GetByIdWithMembershipsAsync(createEnrollmentDto.ClientId);
        if (client == null)
        {
            throw new InvalidOperationException("Client not found.");
        }

        var @class = await _classRepository.GetByIdWithEnrollmentsAsync(createEnrollmentDto.ClassId);
        if (@class == null)
        {
            throw new InvalidOperationException("Class not found.");
        }

        if (@class.Enrollments.Any(e => e.ClientId == createEnrollmentDto.ClientId))
        {
            throw new InvalidOperationException("Client is already enrolled in this class.");
        }

        if (@class.Enrollments.Count >= @class.Capacity)
        {
            throw new InvalidOperationException("Class is full.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasActiveMembership = client.Memberships.Any(m => 
            m.IsActive == true && 
            m.StartDate <= today && 
            m.EndDate >= today);

        if (!hasActiveMembership)
        {
            throw new InvalidOperationException("Client does not have an active membership.");
        }

        var enrollment = new Enrollment
        {
            ClientId = createEnrollmentDto.ClientId,
            ClassId = createEnrollmentDto.ClassId,
            RegistrationTime = DateTime.UtcNow
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return enrollment;
    }
}
