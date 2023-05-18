using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Data.Api.Dtos
{
    public class CustomCommandsDto
    {
        public string CommandName { get; set; }
        public string CommandText { get; set; }
        public DateTime LastUsed { get; set; }
        public long TimesUsed { get; set; }
    }
}
