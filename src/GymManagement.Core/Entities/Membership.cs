using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Membership
{
    public int MembershipId { get; set; }

    public int ClientId { get; set; }

    public int PlanId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Membershipplan Plan { get; set; } = null!;
}
