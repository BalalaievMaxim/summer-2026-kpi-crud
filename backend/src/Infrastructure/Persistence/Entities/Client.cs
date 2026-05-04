using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Infrastructure.Persistence.Entities;

[Table("Client")]
public class Client
{
    [Key]
    [Column("client_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClientId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [Column("phone")]
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
