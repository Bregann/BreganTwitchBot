using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Api.Dtos
{
    public class CustomCommandsDto
    {
        public required string CommandName { get; set; }
        public required string CommandText { get; set; }
        public required string LastUsed { get; set; }
        public required long TimesUsed { get; set; }
    }
}
