using GymManagement.Domain.Shared;

namespace GymManagement.Domain.Memberships.Errors;

public sealed class MembershipNotFoundError : DomainError
{
    public MembershipNotFoundError(int id)
        : base("Membership.NotFound", $"Membership with id '{id}' was not found.") { }
}

public sealed class MembershipAlreadyInactiveError : DomainError
{
    public MembershipAlreadyInactiveError(string message)
        : base("Membership.AlreadyInactive", message) { }
}

public sealed class ActiveMembershipExistsError : DomainError
{
    public ActiveMembershipExistsError(int clientId, int planId)
        : base("Membership.ActiveExists", $"Client '{clientId}' already has an active membership for plan '{planId}'.") { }
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
    public MembershipPlanNotFoundError(int id)
        : base("MembershipPlan.NotFound", $"MembershipPlan with id '{id}' was not found.") { }
}

public sealed class MembershipPlanInUseError : DomainError
{
    public MembershipPlanInUseError(int id)
        : base("MembershipPlan.InUse", $"Cannot delete membership plan '{id}' because active memberships reference it.") { }
}
