using BreganTwitchBot.Domain.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.EventSub.Websockets.Core.EventArgs.Channel;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands
{
    public class CommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, MethodInfo> _commands = new();

        public CommandHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RegisterCommands();
        }

        public void RegisterCommands()
        {
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var commandType in commandTypes)
            {
                var methods = commandType.GetMethods()
                                        .Where(m => m.GetCustomAttributes<TwitchCommandAttribute>() != null);

                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<TwitchCommandAttribute>();

                    if (attribute != null)
                    {
                        _commands["!" + attribute.CommandName] = method;
                    }
                }
            }
        }

        public async Task HandleCommandAsync(string command, ChannelChatMessageArgs context)
        {
            if (_commands.TryGetValue(command, out var method))
            {
                var instance = _serviceProvider.GetRequiredService(method.DeclaringType!);
                if (instance != null)
                {
                    var task = (Task?)method.Invoke(instance, new object[] { context }.ToArray());
                    if (task != null)
                    {
                        await task;
                    }
                }
            }
        }
    }
}
