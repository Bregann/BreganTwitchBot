using BreganTwitchBot.Domain.Data.DiscordBot.Helpers;
using BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.OpenAI;
using BreganTwitchBot.Infrastructure.Database.Models;
using Discord;
using Discord.Interactions;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Modules
{
    public class OpenAIModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly string[] AllowedImageTypes = { "image/png", "image/jpeg" };

        [SlashCommand("ai_analysebooks_openai", "Upload an image to check the book book books against your interests")]
        public async Task AnalyseBooks([Summary("image", "the image to analyse")]IAttachment file)
        {
            await DeferAsync();

            if(!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }

            if (!AllowedImageTypes.Contains(file.ContentType))
            {
                await FollowupAsync("Invalid file type, please upload a png or jpeg image");
                return;
            }

            var response = await OpenAIData.AnalyseBooks(Context.User.Id, file.Url, file.ContentType, false);

            if (response.Length > 1)
            {
                await FollowupAsync(response[0]);

                foreach (var item in response.Skip(1))
                {
                    await DiscordHelper.SendMessage(Context.Channel.Id, item);
                }
            }
        }

        [SlashCommand("ai_analysebooks_gemini", "Upload an image to check the book book books against your interests")]
        public async Task AnalyseBooksGemini([Summary("image", "the image to analyse")] IAttachment file)
        {
            await DeferAsync();

            if (!await OpenAIData.IsAllowedAI(Context.User.Id))
            {
                await FollowupAsync("You don't have permission to use this command");
                return;
            }

            if (!AllowedImageTypes.Contains(file.ContentType))
            {
                await FollowupAsync("Invalid file type, please upload a png or jpeg image");
                return;
            }

            var response = await OpenAIData.AnalyseBooks(Context.User.Id, file.Url, file.ContentType, true);

            if (response.Length > 1)
            {
                await FollowupAsync(response[0]);

                foreach (var item in response.Skip(1))
                {
                    await DiscordHelper.SendMessage(Context.Channel.Id, item);
                }
            }
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
