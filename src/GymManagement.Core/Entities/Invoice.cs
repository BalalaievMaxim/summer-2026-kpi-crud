using System;
using System.Collections.Generic;

namespace GymManagement.Core.Entities;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int ClientId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }

    public string? Status { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public virtual Client Client { get; set; } = null!;
}
