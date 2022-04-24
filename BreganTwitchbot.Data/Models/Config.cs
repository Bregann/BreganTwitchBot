using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Data.Models
{
    public class Config
    {
        [Key]
        [Column("broadcasterName")]
        public string BroadcasterName { get; set; }

        [Column("broadcasterOAuth")]
        public string BroadcasterOAuth { get; set; }

        [Column("broadcasterRefresh")]
        public string BroadcasterRefresh { get; set; }

        [Column("streamAnnounced")]
        public bool StreamAnnounced { get; set; }

        [Column("dailyPointsCollectingAllowed")]
        public bool DailyPointsCollectingAllowed { get; set; }

        [Column("lastDailyPointsAllowed")]
        public DateTime LastDailyPointsAllowed { get; set; }
    }
}
