using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddChannelTimedMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelTimedMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChannelId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinimumChatMessages = table.Column<int>(type: "integer", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    OnlyWhenLive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelTimedMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelTimedMessages_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTimedMessages_ChannelId",
                table: "ChannelTimedMessages",
                column: "ChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelTimedMessages");
        }
    }
}
