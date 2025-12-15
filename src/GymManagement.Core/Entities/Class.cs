using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Core.Entities;

[Table("Class")]
public class Class
{
    [Key]
    [Column("class_id")]
    public int ClassId { get; set; }

    [Column("class_type_id")]
    public int ClassTypeId { get; set; }

    [ForeignKey("ClassTypeId")]
    public ClassType ClassType { get; set; } = null!;

    [Column("coach_id")]
    public int CoachId { get; set; }

    [ForeignKey("CoachId")]
    public Coach Coach { get; set; } = null!;

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime EndTime { get; set; }

    [Column("capacity")]
    public int Capacity { get; set; }

    [Column("current_enrollment")]
    public int CurrentEnrollment { get; set; } = 0;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
