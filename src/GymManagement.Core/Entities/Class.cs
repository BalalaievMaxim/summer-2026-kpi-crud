using System;

namespace GymManagement.Core.Entities;

public partial class Class
{
    public int ClassId { get; set; }

    public int ClassTypeId { get; set; }

    public int CoachId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Capacity { get; set; }

    public int? CurrentEnrollment { get; set; }

    public virtual Classtype ClassType { get; set; } = null!;

    public virtual Coach Coach { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
