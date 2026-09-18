using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddChannelPermissions : Migration
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
                name: "ChannelPermissionGrants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    ChannelUserId = table.Column<int>(type: "integer", nullable: false),
                    Permission = table.Column<int>(type: "integer", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GrantedByTwitchUserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPermissionGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelPermissionGrants_ChannelUsers_ChannelUserId",
                        column: x => x.ChannelUserId,
                        principalTable: "ChannelUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelPermissionGrants_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_ChannelPermissionGrants_ChannelId",
                table: "ChannelPermissionGrants",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPermissionGrants_ChannelUserId",
                table: "ChannelPermissionGrants",
                column: "ChannelUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubathonRates_ChannelId",
                table: "SubathonRates",
                column: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelPermissionGrants");

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
