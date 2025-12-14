using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GymManagement.Core.Entities;

[Table("Membership")]
public class Membership
{
    [Key]
    [Column("membership_id")]
    public int MembershipId { get; set; }

    [Column("client_id")]
    public int ClientId { get; set; }

    [ForeignKey("ClientId")]
    public Client Client { get; set; } = null!;

    [Column("plan_id")]
    public int PlanId { get; set; }

    [ForeignKey("PlanId")]
    public MembershipPlan MembershipPlan { get; set; } = null!;

    [Column("start_date")]
    public DateOnly StartDate { get; set; } // Використовуємо DateOnly для типу SQL DATE

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}
