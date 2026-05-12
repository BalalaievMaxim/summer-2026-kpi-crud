using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.DTOs;

namespace GymManagement.Application.Features.Enrollments.Commands.CreateEnrollment;

public sealed record CreateEnrollmentCommand(int ClientId, int ClassId) : ICommand<int>;