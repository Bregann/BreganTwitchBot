using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BreganTwitchBot.Domain.Database.Models
{
    /// <summary>
    /// How much time bits and subs are worth once a subathon has reached a given number of hours.
    ///
    /// The old bot expressed this as deeply nested switch statements, which meant retuning the
    /// subathon needed a code change and every channel shared one set of rates. The bands are
    /// rows here instead - the band used is the one with the highest FromHours at or below the
    /// subathon's current total.
    /// </summary>
    public class SubathonRate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Channel))]
        [Required]
        public int ChannelId { get; set; }

        public virtual Channel Channel { get; set; } = null!;

        /// <summary>
        /// The band applies once the subathon total reaches this many hours
        /// </summary>
        [Required]
        public required int FromHours { get; set; }

        /// <summary>
        /// Milliseconds added per bit cheered
        /// </summary>
        [Required]
        public required int MillisecondsPerBit { get; set; }

        [Required]
        public required int Tier1SubMinutes { get; set; }

        [Required]
        public required int Tier2SubMinutes { get; set; }

        [Required]
        public required int Tier3SubMinutes { get; set; }
    }
}
