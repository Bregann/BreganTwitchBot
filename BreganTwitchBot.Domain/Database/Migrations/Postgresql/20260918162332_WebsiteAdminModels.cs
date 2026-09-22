using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BreganTwitchBot.Domain.Database.Migrations.Postgresql
{
    /// <inheritdoc />
    public partial class WebsiteAdminModels : Migration
    {
        // This migration was generated on the website branch before the channel point reward and
        // discord giveaway work landed on main. Every table it created (ChannelPointRewards,
        // DiscordGiveaways, DiscordGiveawayConfigs, DiscordGiveawayEntries) is already created by
        // AddChannelPointRewards and AddDiscordGiveaways, so it is left empty rather than deleted
        // to keep the migration chain intact for databases that already recorded it.

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
