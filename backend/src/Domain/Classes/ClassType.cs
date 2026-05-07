using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Classes;

public sealed class ClassType : Entity<int>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private ClassType() { }

    private ClassType(int id, string name, string? description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public static ClassType Create(int id, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidClassTypeError("ClassType name cannot be empty.");

        return new ClassType(id, name.Trim(), description?.Trim());
    }
}
