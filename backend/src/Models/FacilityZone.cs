using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models;

[Table("FacilityZone")]
public class FacilityZone
{
    [Key]
    [Column("zone_id")]
    public int ZoneId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public ICollection<PlanAccess> PlanAccesses { get; set; } = new List<PlanAccess>();
}