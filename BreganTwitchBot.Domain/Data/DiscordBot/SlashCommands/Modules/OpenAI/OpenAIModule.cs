using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.OpenAI;
using BreganTwitchBot.Infrastructure.Database.Models;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules
{
    public class OpenAIModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ai_analysebooks", "Upload an image to check the book book books against your interests")]
        public async Task AnalyseBooks([Summary("image", "the image to analyse")]IAttachment file)
        {
            await DeferAsync();

            if(!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }

            await RespondAsync("pooo");
        }

        [SlashCommand("ai_addlikedauthor", "Add a liked author(s) to your list")]
        public async Task AddLikedItem([Summary("item", "the author to add, multiple can be added by seperating with a comma")] string item)
        {
            await DeferAsync();
            if (!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }
            await OpenAIData.AddNewLikedItem(Context.User.Id, item, AiType.Author);
            await FollowupAsync("Item added to your profile");
        }

        [SlashCommand("ai_addlikedbook", "Add a liked book(s) to your list")]
        public async Task AddLikedBook([Summary("item", "the book to add, multiple can be added by seperating with a comma")] string item)
        {
            await DeferAsync();
            if (!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }
            await OpenAIData.AddNewLikedItem(Context.User.Id, item, AiType.Book);
            await FollowupAsync("Item added to your profile");
        }

        [SlashCommand("ai_addlikedgenre", "Add a liked genre(s) to your list")]
        public async Task AddLikedGenre([Summary("item", "the genre to add, multiple can be added by seperating with a comma")] string item)
        {
            await DeferAsync();
            if (!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }
            await OpenAIData.AddNewLikedItem(Context.User.Id, item, AiType.Genre);
            await FollowupAsync("Item added to your profile");
        }

        [SlashCommand("ai_addlikedseries", "Add a liked series(s) to your list")]
        public async Task AddLikedSeries([Summary("item", "the series to add, multiple can be added by seperating with a comma")] string item)
        {
            await DeferAsync();
            if (!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }
            await OpenAIData.AddNewLikedItem(Context.User.Id, item, AiType.Series);
            await FollowupAsync("Item added to your profile");
        }
    }
}
