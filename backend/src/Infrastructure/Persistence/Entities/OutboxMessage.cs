using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymManagement.Infrastructure.Persistence.Entities;

[Table("outbox_message")]
public sealed class OutboxMessage
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime OccurredOn { get; set; }
    
    public DateTime? ProcessedOn { get; set; }
    
    public string? Error { get; set; }
}