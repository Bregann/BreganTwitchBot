using BreganTwitchBot.Domain.Enums;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IEnvironmentalSettingHelper
    {
        Task LoadEnvironmentalSettings();
        string? TryGetEnviromentalSettingValue(EnvironmentalSettingEnum key);
        Task<bool> UpdateEnviromentalSettingValue(EnvironmentalSettingEnum key, string newValue);
    }
}
