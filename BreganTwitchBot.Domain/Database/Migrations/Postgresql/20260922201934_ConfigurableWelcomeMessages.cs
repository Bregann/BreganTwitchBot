using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class ConfigurableWelcomeMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiscordWelcomeMessageLinked",
                table: "ChannelConfig",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscordWelcomeMessageUnlinked",
                table: "ChannelConfig",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordWelcomeMessageLinked",
                table: "ChannelConfig");

            migrationBuilder.DropColumn(
                name: "DiscordWelcomeMessageUnlinked",
                table: "ChannelConfig");
        }
    }
}
