using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.Persistence.Entities;

namespace Platform.Infrastructure.Persistence.Context;

/// <summary>
/// Extends the auto-generated AppDbContext with Infrastructure-specific tables.
/// Kept separate from the EF Core Power Tools generated file to survive re-scaffolding.
/// </summary>
public partial class AppDbContext
{
    /// <summary>Outbox messages — written atomically with business operations by OutboxInterceptor.</summary>
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    /// <summary>
    /// Configures Infrastructure-specific entities that are not part of the Domain schema.
    /// Called automatically by EF Core as part of the partial OnModelCreating chain.
    /// </summary>
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at").IsRequired();
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
            entity.Property(e => e.Error).HasColumnName("error").HasMaxLength(2000);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(e => e.ClaimedUntil).HasColumnName("claimed_until");
            entity.Property(e => e.ClaimedBy).HasColumnName("claimed_by").HasMaxLength(200);

            // Index for fast polling: unprocessed messages ordered by OccurredAt
            entity.HasIndex(e => new { e.ProcessedAt, e.ClaimedUntil, e.OccurredAt })
                  .HasFilter("processed_at IS NULL")
                  .HasDatabaseName("ix_outbox_messages_unprocessed");
        });
    }
}
