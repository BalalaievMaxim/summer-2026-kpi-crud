using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GymManagement.Core.Entities;

[Table("MembershipPlan")]
public class MembershipPlan
{
    [Key]
    [Column("plan_id")]
    public int PlanId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("duration_months")]
    public int DurationMonths { get; set; }

    [Column("price", TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    public ICollection<PlanAccess> PlanAccesses { get; set; } = new List<PlanAccess>();
    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}