using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Coach
{
    public int CoachId { get; set; }

    public string Name { get; set; } = null!;

    public string Specialization { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
