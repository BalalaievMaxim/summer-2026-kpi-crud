using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Facilityzone
{
    public int ZoneId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Membershipplan> Plans { get; set; } = new List<Membershipplan>();
}
