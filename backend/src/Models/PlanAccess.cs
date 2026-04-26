using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Models;

[Table("PlanAccess")]
public class PlanAccess
{
    [Column("plan_id")]
    public int PlanId { get; set; }

    [ForeignKey("PlanId")]
    public MembershipPlan MembershipPlan { get; set; } = null!;

    [Column("zone_id")]
    public int ZoneId { get; set; }

    [ForeignKey("ZoneId")]
    public FacilityZone FacilityZone { get; set; } = null!;
}