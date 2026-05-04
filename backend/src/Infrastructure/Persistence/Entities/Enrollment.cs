using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GymManagement.Infrastructure.Persistence.Entities;

[Table("Enrollment")]
public class Enrollment
{
    [Key]
    [Column("enrollment_id")]
    public int EnrollmentId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;

    [Column("class_id")]
    public int ClassId { get; set; }

    [ForeignKey("ClassId")]
    public Class Class { get; set; } = null!;

    [Column("registration_time")]
    public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
}