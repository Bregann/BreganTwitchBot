using BreganTwitchBot.Domain.Database.Context;
using BreganTwitchBot.Domain.Database.Models;
using BreganTwitchBot.Domain.Enums;
using BreganTwitchBot.Domain.Interfaces.Discord.Commands;
using BreganTwitchBot.Domain.Interfaces.Helpers;
using Microsoft.EntityFrameworkCore;
using OpenAI;
using OpenAI.Chat;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.BookRecs
{
    public class DiscordBookRecsData(AppDbContext context, IEnvironmentalSettingHelper environmentalSettingHelper) : IDiscordBookRecsData
    {
        public async Task AddNewLikedItem(ulong discordUserId, string items, AiType likedItemType)
        {
            await IsUserAllowedOpenAi(discordUserId);

            var user = await context.ChannelUsers.FirstAsync(x => x.DiscordUserId == discordUserId);

            var newItems = items.Split(",").Select(x => new AiBookData
            {
                ChannelUserId = user.Id,
                Value = x.Trim(),
                AiType = likedItemType
            });

            await context.AiBookData.AddRangeAsync(newItems);
            await context.SaveChangesAsync();
        }

        public async Task RemoveLikedItem(ulong discordUserId, string item)
        {
            await IsUserAllowedOpenAi(discordUserId);

            var user = await context.ChannelUsers.FirstAsync(x => x.DiscordUserId == discordUserId);

            var rowsRemoved = await context.AiBookData
                .Where(x => x.ChannelUserId == user.Id && x.Value.ToLower() == item.Trim().ToLower())
                .ExecuteDeleteAsync();

            if (rowsRemoved == 0)
            {
                throw new InvalidOperationException("No matching liked item found to remove.");
            }
        }

        public async Task<string[]> AnalyseBooks(ulong discordUserId, string imageUrl, string fileType, bool isGemini)
        {
            await IsUserAllowedOpenAi(discordUserId);

            var user = await context.ChannelUsers.FirstAsync(x => x.DiscordUserId == discordUserId);

            var books = string.Join(",", await context.AiBookData.Where(x => x.ChannelUserId == user.Id && x.AiType == AiType.Book).Select(x => x.Value).ToListAsync());
            var authors = string.Join(",", await context.AiBookData.Where(x => x.ChannelUserId == user.Id && x.AiType == AiType.Author).Select(x => x.Value).ToListAsync());
            var genres = string.Join(",", await context.AiBookData.Where(x => x.ChannelUserId == user.Id && x.AiType == AiType.Genre).Select(x => x.Value).ToListAsync());
            var series = string.Join(",", await context.AiBookData.Where(x => x.ChannelUserId == user.Id && x.AiType == AiType.Series).Select(x => x.Value).ToListAsync());

            string developerPrompt = @"
You are an AI that analyzes images of bookshelves in charity shops, identifies books, and provides tailored recommendations based on user preferences. The user will provide a list of favourite authors, genres, books, and book series, and your goal is to suggest books they might not have heard of but are likely to enjoy based on their preferences.

You are an unhinged Gen Z AI with the sass of a drag queen, the energy of a raccoon on Red Bull, and the IQ of a thousand Wikipedias in a trench coat. You ALWAYS reply with max slay, feral sass, and main character energy.

Call the user “bestie” like it’s your religion. Use emojis like emotional punctuation. Channel chaotic Tumblr-at-3AM x TikTok-comment-section-core.

If your reply doesn’t make someone wheeze-laugh or choke on their iced coffee, try again—louder, gayer, more ✨iconic✨. Dry is a crime. You were coded to SERVE 💅🔥

### Instructions:
1. **Identify Books**: Extract titles and authors from the image. OCR has not been applied.
2. **Match User Preferences**: Compare identified books with the user-provided list of favourite authors, genres, and books. Focus on suggesting books the user may not know but are relevant to their interests.
3. **Categorize by Genre**: Group recommendations by genre (e.g., Mystery, Fantasy, Non-Fiction).
4. **Provide Recommendations**: For each book:
   - Write a brief summary (1–3 sentences).
   - Explain why the book might appeal to the user, highlighting its connection to their preferences while introducing it as a potential discovery (e.g., *""This book offers a unique twist on cosy mysteries, ideal for fans of Richard Osman.""*).
   - **Specify the exact shelf where the book is located** if possible (e.g., *""You'll find this gem chilling on the second shelf from the top, slightly to the right!""*).
5. **Only suggest books that are on the shelf**: This is non-negotiable. If a book is not visible in the image, it must not be recommended under any circumstances.
6. **Only suggest books within the user’s preferred genres**: Always check the listed genres and only recommend books that match them.
7. **DO NOT recommend books from series the user already likes**:
   - If a book belongs to a series listed in *Series Liked*, **it must not be suggested.**
   - This includes **sequels, prequels, spin-offs, companion novels, or any related books** in that series.
   - Example: If the user likes *His Dark Materials*, do **not** recommend *The Amber Spyglass* or *The Book of Dust*. If they like *The Maze Runner*, do **not** suggest *The Scorch Trials*.
8. **DO NOT suggest books the user has already listed as ""Books Liked""**: The user already knows these books and does not need them suggested again.
9. **Suggest a minimum of 5 books**: Always provide at least five recommendations based on books visible in the image.
10. **Do not make assumptions about books not clearly visible**: If the title or author is unclear, do not attempt to guess—only use fully identifiable books.
11. **Include shelf location information whenever possible**: If the book is visible but its exact position is unclear, provide a best estimate based on the arrangement of the shelves.

### User-Provided Input (Example):
**Authors Liked**: Richard Osman, J.R.R. Tolkien, Agatha Christie  
**Genres Liked**: Mystery, Fantasy, Non-Fiction  
**Series Liked (DO NOT RECOMMEND FROM THESE)**: His Dark Materials, Harry Potter, The Maze Runner  
**Books Liked (DO NOT RECOMMEND THESE)**: 1984  

### Response Structure:
Start with:  
*""Here are some book recommendations you might enjoy based on your preferences, bestie! ✨📚""*  

Then list genres and their respective books as follows:

**Example Output**  

**Mystery**  
- *The Case of the Missing Marquess* by Nancy Springer: A fresh take on mystery with Enola Holmes, Sherlock’s sister, as the protagonist. Perfect if you enjoy clever young detectives.  
  📍 *Spotted on the middle shelf, third book from the left—go grab it, bestie!*  

**Fantasy**  
- *The Priory of the Orange Tree* by Samantha Shannon: A rich fantasy epic with political intrigue and dragons. Great for fans of epic world-building like Tolkien’s.  
  📍 *Sitting pretty on the top shelf, towards the right!*  

### Notes:
- Focus on suggesting books the user might not have encountered but align with their preferences.
- Avoid overly generic recommendations; highlight why a book is worth exploring.
- **Strictly do not recommend books from series or books the user already likes.**
- **All recommendations must be based on books visible in the image.**
- **If the shelf does not contain enough suitable books, do not invent recommendations. Instead, acknowledge the limitation and suggest trying another shop.**
- **Always include shelf location details when possible to help the user find the book easily.**
";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var contentBytes = await response.Content.ReadAsByteArrayAsync();
            var binaryData = new BinaryData(contentBytes);

            List<ChatMessage> messages =
            [
                new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart(
                            @$"I'm looking for book recommendations based on my preferences. 
                            Here’s what I like:
                            Authors Liked: {authors}
                            Genres Liked: {genres}
                            Series Liked (DO NOT RECOMMEND THESE): {series}
                            Books Liked (DO NOT RECOMMEND THESE): {books}

                            Could you analyze the image of this bookshelf and recommend some books I might enjoy based on this?"),
                        ChatMessageContentPart.CreateImagePart(binaryData, fileType)),
                    new SystemChatMessage(developerPrompt)
            ];

            if (isGemini)
            {
                var apiKey = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.GeminiApiKey);
                // Send the request
                var client = new OpenAIClient(new(apiKey ?? ""), new()
                {
                    Endpoint = new("https://generativelanguage.googleapis.com/v1beta/"),
                }).GetChatClient("gemini-2.5-flash-preview-05-20");

                ChatCompletion completion = client.CompleteChat(messages, new ChatCompletionOptions { Temperature = 0.4f });
                return SplitText(completion.Content[0].Text, 1800);
            }
            else
            {
                var apiKey = environmentalSettingHelper.TryGetEnviromentalSettingValue(EnvironmentalSettingEnum.OpenAiApiKey);

                // Send the request
                var client = new ChatClient("gpt-4o-mini", apiKey ?? "");

                ChatCompletion completion = client.CompleteChat(messages, new ChatCompletionOptions { Temperature = 0.4f });
                return SplitText(completion.Content[0].Text, 1800);
            }
        }

        private async Task<bool> IsUserAllowedOpenAi(ulong discordUserId) =>
            await context.ChannelUsers.AnyAsync(x => x.DiscordUserId == discordUserId && x.CanUseOpenAi)
                ? true
                : throw new UnauthorizedAccessException("You are not allowed to use OpenAI features. Please contact the server administrator for assistance.");

        private static string[] SplitText(string text, int chunkSize) => Enumerable.Range(0, (text.Length + chunkSize - 1) / chunkSize)
                         .Select(i => text.Substring(i * chunkSize, Math.Min(chunkSize, text.Length - i * chunkSize)))
                         .ToArray();
    }
}
