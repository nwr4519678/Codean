using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Platform.Infrastructure.Persistence.Migrations;

public partial class AddOutboxClaims : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>("claimed_until", "outbox_messages", type: "timestamp with time zone", nullable: true);
        migrationBuilder.AddColumn<string>("claimed_by", "outbox_messages", type: "character varying(200)", maxLength: 200, nullable: true);
        migrationBuilder.CreateIndex("ix_outbox_messages_claimable", "outbox_messages", new[] { "processed_at", "claimed_until", "occurred_at" }, filter: "processed_at IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("ix_outbox_messages_claimable", "outbox_messages");
        migrationBuilder.DropColumn("claimed_until", "outbox_messages");
        migrationBuilder.DropColumn("claimed_by", "outbox_messages");
    }
}
