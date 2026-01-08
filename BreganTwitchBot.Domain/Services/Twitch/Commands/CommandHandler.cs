using BreganTwitchBot.Domain.Attributes;
using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.DTOs.Twitch.EventSubEvents;
using BreganTwitchBot.Domain.Interfaces.Twitch;
using BreganTwitchBot.Domain.Interfaces.Twitch.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BreganTwitchBot.Domain.Services.Twitch.Commands
{
    public class CommandHandler : ICommandHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, MethodInfo> _commands = new();

        // Custom commands are stored in a list of tuples, where the first item is the command name and the second item is the broadcaster id
        // This is to prevent the need to query the database for every command, a dictionary would be better but it's not possible to have a dictionary with multiple keys
        // incase multiple broadcasters have the same command name
        private List<(string, string)> _customCommands = new();

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
                        _commands["!" + attribute.CommandName.ToLower()] = method;

                        if (attribute.CommandAlias != null)
                        {
                            foreach (var alias in attribute.CommandAlias)
                            {
                                _commands["!" + alias.ToLower()] = method;
                            }
                        }
                    }
                }
            }
        }

        public void LoadCustomCommands()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                _customCommands = context.CustomCommands
                    .AsEnumerable()
                    .Select(x => (x.CommandName, x.Channel.BroadcasterTwitchChannelId))
                    .ToList();
            }
        }

        public async Task HandleCommandAsync(string command, ChannelChatMessageReceivedParams msgParams)
        {
            // For the predefined commands
            if (_commands.TryGetValue(command.ToLower(), out var method))
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
            if (_customCommands.Any(x => x.Item1 == command.ToLower().Trim() && x.Item2 == msgParams.BroadcasterChannelId))
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
            return _commands.ContainsKey(commandName.ToLower().Trim('!'));
        }

        public bool RemoveCustomCommand(string commandName, string broadcasterId)
        {
            var command = _customCommands.FirstOrDefault(x => x.Item1 == commandName && x.Item2 == broadcasterId);
            return _customCommands.Remove(command);
        }

        public void AddCustomCommand(string commandName, string broadcasterId)
        {
            if (!_customCommands.Any(x => x.Item1 == commandName))
            {
                _customCommands.Add(new(commandName, broadcasterId));
            }
        }
    }
}
