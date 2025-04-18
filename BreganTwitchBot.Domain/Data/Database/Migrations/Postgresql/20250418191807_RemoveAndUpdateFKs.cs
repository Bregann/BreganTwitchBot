using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Domain.Data.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class RemoveAndUpdateFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiscordLinkRequests_Channels_ChannelId",
                table: "DiscordLinkRequests");

            migrationBuilder.DropIndex(
                name: "IX_DiscordLinkRequests_ChannelId",
                table: "DiscordLinkRequests");

            migrationBuilder.DropIndex(
                name: "IX_DiscordDailyPoints_ChannelUserId",
                table: "DiscordDailyPoints");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "DiscordLinkRequests");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyPoints_ChannelUserId",
                table: "DiscordDailyPoints",
                column: "ChannelUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscordDailyPoints_ChannelUserId",
                table: "DiscordDailyPoints");

            migrationBuilder.AddColumn<int>(
                name: "ChannelId",
                table: "DiscordLinkRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordLinkRequests_ChannelId",
                table: "DiscordLinkRequests",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyPoints_ChannelUserId",
                table: "DiscordDailyPoints",
                column: "ChannelUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DiscordLinkRequests_Channels_ChannelId",
                table: "DiscordLinkRequests",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }
    }
}
