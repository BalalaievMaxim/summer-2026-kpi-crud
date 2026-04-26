using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GymManagement.Models;

[Table("Coach")]
public class Coach
{
    [Key]
    [Column("coach_id")]
    public int CoachId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("specialization")]
    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Column("email")]
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
