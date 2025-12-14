using GymManagement.Core.Interfaces;

namespace GymManagement.Application.Services;

public class EnrollmentService
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
}
