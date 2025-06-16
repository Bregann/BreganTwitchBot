using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class FinalReleaseMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordEnabled",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "DiscordGuildId",
                table: "Channels");

            migrationBuilder.AddColumn<bool>(
                name: "DiscordEnabled",
                table: "ChannelConfig",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGuildId",
                table: "ChannelConfig",
                type: "numeric(20,0)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordEnabled",
                table: "ChannelConfig");

            migrationBuilder.DropColumn(
                name: "DiscordGuildId",
                table: "ChannelConfig");

            migrationBuilder.AddColumn<bool>(
                name: "DiscordEnabled",
                table: "Channels",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscordGuildId",
                table: "Channels",
                type: "numeric(20,0)",
                nullable: true);
        }
    }
}
