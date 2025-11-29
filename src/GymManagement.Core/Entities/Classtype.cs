using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Classtype
{
    public int ClassTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
