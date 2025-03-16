using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.Data.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BreganTwitchBot.Domain.Data.Services.Twitch.Commands
{
    public class CommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, MethodInfo> _commands = new();
        private List<string> _customCommands = new();

        public CommandHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            RegisterCommands();
            LoadCustomCommands();
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

                        if (attribute.CommandAlias != null)
                        {
                            foreach (var alias in attribute.CommandAlias)
                            {
                                _commands["!" + alias] = method;
                            }
                        }
                    }
                }
            }
        }

        public void LoadCustomCommands()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _customCommands = context.CustomCommands.Select(x => x.CommandName).ToList();
            }
        }

        public async Task HandleCommandAsync(string command, ChannelChatMessageReceivedParams msgParams)
        {
            // For the predefined commands
            if (_commands.TryGetValue(command, out var method))
            {
                var instance = _serviceProvider.GetRequiredService(method.DeclaringType!);
                if (instance != null)
                {
                    var task = (Task?)method.Invoke(instance, new object[] { msgParams }.ToArray());
                    if (task != null)
                    {
                        await task;
                    }
                }
            }

            //For custom commands, use the custom command service
            if (_customCommands.Contains(command.ToLower().Trim()))
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var customCommandService = scope.ServiceProvider.GetRequiredService<ICustomCommandDataService>();
                    await customCommandService.HandleCustomCommandAsync(command, msgParams);
                }
            }
        }

        public bool IsSystemCommand(string commandName)
        {
            return _commands.ContainsKey(commandName.Trim('!'));
        }

        public bool RemoveCustomCommand(string commandName)
        {
            return _customCommands.Remove(commandName);
        }

        public void AddCustomCommand(string commandName)
        {
            if (!_customCommands.Contains(commandName))
            {
                _customCommands.Add(commandName);
            }
        }
    }
}
