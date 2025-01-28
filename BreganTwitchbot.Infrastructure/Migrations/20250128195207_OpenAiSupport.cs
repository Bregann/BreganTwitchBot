using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BreganTwitchBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OpenAiSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanUseOpenAi",
                schema: "twitchbot_new",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OpenAiApiKey",
                schema: "twitchbot_new",
                table: "Config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AiBookData",
                schema: "twitchbot_new",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TwitchUserId = table.Column<string>(type: "text", nullable: false),
                    AiType = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiBookData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiBookData_Users_TwitchUserId",
                        column: x => x.TwitchUserId,
                        principalSchema: "twitchbot_new",
                        principalTable: "Users",
                        principalColumn: "TwitchUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "OpenAiApiKey", "PinnedStreamDate" },
                values: new object[] { new DateTime(2025, 1, 27, 19, 52, 6, 926, DateTimeKind.Utc).AddTicks(7659), "", new DateTime(2025, 1, 27, 19, 52, 6, 926, DateTimeKind.Utc).AddTicks(7655) });

            migrationBuilder.CreateIndex(
                name: "IX_AiBookData_TwitchUserId",
                schema: "twitchbot_new",
                table: "AiBookData",
                column: "TwitchUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiBookData",
                schema: "twitchbot_new");

            migrationBuilder.DropColumn(
                name: "CanUseOpenAi",
                schema: "twitchbot_new",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OpenAiApiKey",
                schema: "twitchbot_new",
                table: "Config");

            migrationBuilder.UpdateData(
                schema: "twitchbot_new",
                table: "Config",
                keyColumn: "BroadcasterName",
                keyValue: "",
                columns: new[] { "LastDailyPointsAllowed", "PinnedStreamDate" },
                values: new object[] { new DateTime(2023, 12, 29, 19, 36, 57, 63, DateTimeKind.Utc).AddTicks(8889), new DateTime(2023, 12, 29, 19, 36, 57, 63, DateTimeKind.Utc).AddTicks(8884) });
        }
    }
}
