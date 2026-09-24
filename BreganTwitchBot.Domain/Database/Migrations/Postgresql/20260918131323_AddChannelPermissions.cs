using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class AddChannelPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPermissionGrants_ChannelId",
                table: "ChannelPermissionGrants",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelPermissionGrants_ChannelUserId",
                table: "ChannelPermissionGrants",
                column: "ChannelUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelPermissionGrants");
        }
    }
}
