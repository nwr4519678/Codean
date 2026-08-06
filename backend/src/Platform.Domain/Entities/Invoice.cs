#nullable disable
using System;
using System.Collections.Generic;

namespace Platform.Domain.Entities;

public partial class Invoice
{
    public long Id { get; set; }

    public long PaymentId { get; set; }

    public string InvoiceNumber { get; set; }

    public DateTime IssuedAt { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public string PdfUrl { get; set; }

    public virtual Payment Payment { get; set; }
}