using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using Discord;
using Discord.Interactions;
using Serilog;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.BookRecs
{
    public class DiscordRecsModule(IDiscordBookRecsData discordBookRecsData, IDiscordHelperService discordHelperService) : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly string[] AllowedImageTypes = ["image/png", "image/jpeg"];

        [SlashCommand("ai_analysebooks_openai", "Upload an image to check the book book books against your interests")]
        public async Task AnalyseBooks([Summary("image", "the image to analyse")] IAttachment file)
        {
            await DeferAsync();

            if (!AllowedImageTypes.Contains(file.ContentType))
            {
                await FollowupAsync("Invalid file type, please upload a png or jpeg image");
                return;
            }

            var response = await discordBookRecsData.AnalyseBooks(Context.User.Id, file.Url, file.ContentType, false);

            if (response.Length > 1)
            {
                await FollowupAsync(response[0]);

                foreach (var item in response.Skip(1))
                {
                    await discordHelperService.SendMessage(Context.Channel.Id, item);
                }
            }
            else
            {
                await FollowupAsync(response[0]);
            }
        }

        [SlashCommand("ai_analysebooks_gemini", "Upload an image to check the book book books against your interests using Gemini")]
        public async Task AnalyseBooksGemini([Summary("image", "the image to analyse")] IAttachment file)
        {
            await DeferAsync();
            if (!AllowedImageTypes.Contains(file.ContentType))
            {
                await FollowupAsync("Invalid file type, please upload a png or jpeg image");
                return;
            }
            var response = await discordBookRecsData.AnalyseBooks(Context.User.Id, file.Url, file.ContentType, true);
            if (response.Length > 1)
            {
                await FollowupAsync(response[0]);
                foreach (var item in response.Skip(1))
                {
                    await discordHelperService.SendMessage(Context.Channel.Id, item);
                }
            }
            else
            {
                await FollowupAsync(response[0]);
            }
        }

        [SlashCommand("ai_addlikedauthor", "Add a liked author(s) to your list")]
        public async Task AddLikedAuthor([Summary("authors", "the author(s) to add, separated by commas")] string authors)
        {
            await DeferAsync();

            var response = await AddItem(authors, AiType.Author);
            await FollowupAsync(response);
        }

        [SlashCommand("ai_addlikedbook", "Add a liked book(s) to your list")]
        public async Task AddLikedBook([Summary("books", "the book(s) to add, separated by commas")] string books)
        {
            await DeferAsync();

            var response = await AddItem(books, AiType.Book);
            await FollowupAsync(response);
        }

        [SlashCommand("ai_addlikedseries", "Add a liked series(es) to your list")]
        public async Task AddLikedSeries([Summary("series", "the series(es) to add, separated by commas")] string series)
        {
            await DeferAsync();

            var response = await AddItem(series, AiType.Series);
            await FollowupAsync(response);
        }

        [SlashCommand("ai_addlikedgenre", "Add a liked genre(s) to your list")]
        public async Task AddLikedGenre([Summary("genres", "the genre(s) to add, separated by commas")] string genres)
        {
            await DeferAsync();

            var response = await AddItem(genres, AiType.Genre);
            await FollowupAsync(response);
        }

        [SlashCommand("ai_removelikeditem", "Remove a liked item from your list")]
        public async Task RemoveLikedItem([Summary("item", "the item to remove")] string item)
        {
            await DeferAsync();

            if (string.IsNullOrWhiteSpace(item))
            {
                await FollowupAsync("You must provide an item to remove");
                return;
            }

            try
            {
                await discordBookRecsData.RemoveLikedItem(Context.User.Id, item);
                await FollowupAsync($"Removed liked item: {item}");
            }
            catch (UnauthorizedAccessException ex)
            {
                await FollowupAsync($"You do not have permission to remove liked items: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error removing liked item for user {Context.User.Id}");
                await FollowupAsync("An error occurred while removing the liked item");
            }
        }

        private async Task<string> AddItem(string item, AiType itemType)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return "You must provide an item to add";
            }
            try
            {
                await discordBookRecsData.AddNewLikedItem(Context.User.Id, item, itemType);
            }
            catch (UnauthorizedAccessException ex)
            {
                return $"You do not have permission to add liked items: {ex.Message}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error adding liked item for user {Context.User.Id}");
                return "An error occurred while adding the liked item";
            }
            return $"Added liked {itemType.ToString().ToLower()}: {item}";
        }
    }
}
