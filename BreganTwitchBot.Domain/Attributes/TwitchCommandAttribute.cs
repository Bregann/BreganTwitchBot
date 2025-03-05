using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BreganTwitchBot.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TwitchCommandAttribute : Attribute
    {
        public string CommandName { get; }
        public string[]? CommandAlias { get; }

        public TwitchCommandAttribute(string commandName, string[]? commandAlias)
        {
            CommandName = commandName.ToLower();
            CommandAlias = commandAlias;
        }
    }
}
