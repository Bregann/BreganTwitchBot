using BreganTwitchBot.Infrastructure.Database.Context;

namespace BreganTwitchBot.Web.Data.Stats
{
    public class GamblingStatsService
    {
        public GamblingStats GetSlotMachineStatInfo()
        {
            using (var context = new DatabaseContext())
            {
                var stats = context.SlotMachine.First();

                return new GamblingStats
                {
                    DiscordTotalSpins = stats.DiscordTotalSpins,
                    CheeseWins = stats.CheeseWins,
                    CherriesWins = stats.CherriesWins,
                    CucumberWins = stats.CucumberWins,
                    EggplantWins = stats.EggplantWins,
                    GrapesWins = stats.GrapesWins,
                    JackpotAmount = stats.JackpotAmount,
                    JackpotWins = stats.JackpotWins,
                    PineappleWins = stats.PineappleWins,
                    SmorcWins = stats.SmorcWins,
                    Tier1Wins = stats.Tier1Wins,
                    Tier2Wins = stats.Tier2Wins,
                    Tier3Wins = stats.Tier3Wins,
                    TotalSpins = stats.TotalSpins
                };
            }
        }
    }
}