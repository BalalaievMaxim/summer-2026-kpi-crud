using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Memberships.Errors;

public sealed class MembershipNotFoundError : DomainError
{
    public MembershipNotFoundError(Guid id)
        : base("Membership.NotFound", $"Membership with id '{id}' was not found.") { }
}

public sealed class MembershipAlreadyInactiveError : DomainError
{
    public MembershipAlreadyInactiveError(string message)
        : base("Membership.AlreadyInactive", message) { }
}

public sealed class ActiveMembershipExistsError : DomainError
{
    public ActiveMembershipExistsError(Guid clientId)
        : base("Membership.ActiveExists", $"Client '{clientId}' already has an active membership.") { }
}

public sealed class InvalidMembershipError : DomainError
{
    public InvalidMembershipError(string message)
        : base("Membership.Invalid", message) { }
}

public sealed class InvalidMembershipPlanError : DomainError
{
    public InvalidMembershipPlanError(string message)
        : base("MembershipPlan.Invalid", message) { }
}

public sealed class MembershipPlanNotFoundError : DomainError
{
    public MembershipPlanNotFoundError(Guid id)
        : base("MembershipPlan.NotFound", $"MembershipPlan with id '{id}' was not found.") { }
}
