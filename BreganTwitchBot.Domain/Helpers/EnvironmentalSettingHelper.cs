using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BreganTwitchBot.Domain.Helpers
{
    public class EnvironmentalSettingHelper(IServiceProvider serviceProvider) : IEnvironmentalSettingHelper
    {
        private Dictionary<EnvironmentalSettingEnum, string> _environmentalSettings = [];

        public async Task LoadEnvironmentalSettings()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _environmentalSettings = await dbContext.EnvironmentalSettings.ToDictionaryAsync(x => x.Key, x => x.Value);
            }
        }

        public string? TryGetEnviromentalSettingValue(EnvironmentalSettingEnum key)
        {
            return _environmentalSettings.TryGetValue(key, out var value) ? value : null;
        }

        public async Task<bool> UpdateEnviromentalSettingValue(EnvironmentalSettingEnum key, string newValue)
        {
            if (!_environmentalSettings.ContainsKey(key))
            {
                return false;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.EnvironmentalSettings.Where(x => x.Key == key).First().Value = newValue;
                await dbContext.SaveChangesAsync();
            }

            _environmentalSettings[key] = newValue;
            return true;
        }
    }
}
