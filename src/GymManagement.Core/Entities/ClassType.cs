using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Core.Entities;

[Table("ClassType")]
public class ClassType
{
    [Key]
    [Column("class_type_id")]
    public int ClassTypeId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}