using GymManagement.Domain.Shared;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Domain.Coaches;

public sealed class Coach : AggregateRoot<int>
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public SpecializationName Specialization { get; private set; } = null!;
    public int UserId { get; private set; }

    private Coach() { }

    private Coach(int id, string firstName, string lastName, SpecializationName specialization, int userId)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Specialization = specialization;
        UserId = userId;
    }

    public static Coach Create(int id, string firstName, string lastName, string specialization, int userId)
    {
        var specializationVo = SpecializationName.Create(specialization);
        return new Coach(id, firstName, lastName, specializationVo, userId);
    }

    public void UpdateSpecialization(string specialization)
    {
        Specialization = SpecializationName.Create(specialization);
    }
}
