using System;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SystemMain.Migrations.EventSourcing
{
    /// <inheritdoc />
    public partial class EventSourcingInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSourcingEvent",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    time_stamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    aggregate_identifier = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_type = table.Column<string>(type: "text", nullable: false),
                    event_operator = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    event_data = table.Column<JsonNode>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSourcingEvent", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_EventSourcingEvents_AggregateIdentifier_Version",
                table: "EventSourcingEvent",
                columns: new[] { "aggregate_identifier", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSourcingEvent");
        }
    }
}
