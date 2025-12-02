using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Membershipplan
{
    public int PlanId { get; set; }

    public string Name { get; set; } = null!;

    public int DurationMonths { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public virtual ICollection<Facilityzone> Zones { get; set; } = new List<Facilityzone>();
}
