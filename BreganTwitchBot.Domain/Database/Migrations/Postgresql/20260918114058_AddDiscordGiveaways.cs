using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddDiscordGiveaways : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordGiveawayConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    MinutesPerEntry = table.Column<int>(type: "integer", nullable: false),
                    XpPerEntry = table.Column<int>(type: "integer", nullable: false),
                    MaxXpEntries = table.Column<int>(type: "integer", nullable: false),
                    RanksGrantEntries = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGiveawayConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordGiveawayConfigs_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGiveaways",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    GiveawayId = table.Column<string>(type: "text", nullable: false),
                    StartedByDiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    MinimumWatchtimeMinutes = table.Column<int>(type: "integer", nullable: false),
                    RequiredRankId = table.Column<int>(type: "integer", nullable: true),
                    WinnerDiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGiveaways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordGiveaways_ChannelRanks_RequiredRankId",
                        column: x => x.RequiredRankId,
                        principalTable: "ChannelRanks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiscordGiveaways_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscordGiveawayEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordGiveawayId = table.Column<int>(type: "integer", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Entries = table.Column<int>(type: "integer", nullable: false),
                    EnteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordGiveawayEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordGiveawayEntries_DiscordGiveaways_DiscordGiveawayId",
                        column: x => x.DiscordGiveawayId,
                        principalTable: "DiscordGiveaways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGiveawayConfigs_ChannelId",
                table: "DiscordGiveawayConfigs",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGiveawayEntries_DiscordGiveawayId",
                table: "DiscordGiveawayEntries",
                column: "DiscordGiveawayId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGiveaways_ChannelId",
                table: "DiscordGiveaways",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordGiveaways_RequiredRankId",
                table: "DiscordGiveaways",
                column: "RequiredRankId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordGiveawayConfigs");

            migrationBuilder.DropTable(
                name: "DiscordGiveawayEntries");

            migrationBuilder.DropTable(
                name: "DiscordGiveaways");
        }
    }
}
