using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Data.Database.Models;
using BreganTwitchBot.Domain.Interfaces.Helpers;

namespace BreganTwitchBot.Domain.Helpers
{
    public class DatabaseSeedHelper
    {
        public static async Task SeedDatabase(AppDbContext context, IEnvironmentalSettingHelper settingsHelper, IServiceProvider serviceProvider)
        {
            // Generate the data
            // blah blah blah

            await context.EnvironmentalSettings.AddAsync(new EnvironmentalSetting
            {
                Key = Enums.EnvironmentalSettingEnum.HangfireUsername,
                Value = "admin"
            });

            await context.EnvironmentalSettings.AddAsync(new EnvironmentalSetting
            {
                Key = Enums.EnvironmentalSettingEnum.HangfirePassword,
                Value = "password"
            });

            await context.SaveChangesAsync();
        }
    }
}
