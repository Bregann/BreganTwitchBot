using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class SubathonPerChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BitsDonated",
                table: "Subathons",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SubsGifted",
                table: "Subathons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeAdded",
                table: "Subathons",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateTime>(
                name: "SubathonStartTime",
                table: "ChannelConfig",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubathonRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    FromHours = table.Column<int>(type: "integer", nullable: false),
                    MillisecondsPerBit = table.Column<int>(type: "integer", nullable: false),
                    Tier1SubMinutes = table.Column<int>(type: "integer", nullable: false),
                    Tier2SubMinutes = table.Column<int>(type: "integer", nullable: false),
                    Tier3SubMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubathonRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubathonRates_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubathonRates_ChannelId",
                table: "SubathonRates",
                column: "ChannelId");

            // Seed every existing channel with the rates the old bot hardcoded, so a subathon
            // started straight after this migration behaves the same as before. These are per
            // channel rows and can be retuned without a deploy.
            migrationBuilder.Sql(@"
                INSERT INTO ""SubathonRates"" (""ChannelId"", ""FromHours"", ""MillisecondsPerBit"", ""Tier1SubMinutes"", ""Tier2SubMinutes"", ""Tier3SubMinutes"")
                SELECT c.""Id"", v.from_hours, v.ms_per_bit, v.t1, v.t2, v.t3
                FROM ""Channels"" c
                CROSS JOIN (VALUES
                    (0,  900, 6, 12, 30),
                    (12, 750, 5, 10, 25),
                    (13, 600, 4, 8,  20),
                    (16, 450, 3, 6,  12),
                    (23, 300, 2, 4,  8),
                    (24, 150, 1, 2,  5)
                ) AS v(from_hours, ms_per_bit, t1, t2, t3);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubathonRates");

            migrationBuilder.DropColumn(
                name: "BitsDonated",
                table: "Subathons");

            migrationBuilder.DropColumn(
                name: "SubsGifted",
                table: "Subathons");

            migrationBuilder.DropColumn(
                name: "TimeAdded",
                table: "Subathons");

            migrationBuilder.DropColumn(
                name: "SubathonStartTime",
                table: "ChannelConfig");
        }
    }
}
