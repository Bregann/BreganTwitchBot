using BreganTwitchBot.Infrastructure.Database.Context;
using BreganTwitchBot.Infrastructure.Database.Models;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace BreganTwitchBot.Domain.Data.DiscordBot.SlashCommands.Data.OpenAI
{
    public class OpenAIData
    {
        public static async Task<bool> IsAllowedAI(ulong discordUserId)
        {
            using (var context = new DatabaseContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
                if (user == null)
                {
                    return false;
                }
                return user.CanUseOpenAi;
            }
        }

        public static async Task AddNewLikedItem(ulong discordUserId, string items, AiType likedItemType)
        {
            using (var context = new DatabaseContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
                if (user == null)
                {
                    return;
                }

                var newItems = items.Split(",").Select(x => new AiBookData
                {
                    TwitchUserId = user.TwitchUserId,
                    Value = x.Trim(),
                    AiType = likedItemType
                });

                await context.AiBookData.AddRangeAsync(newItems);
                await context.SaveChangesAsync();
            }
        }

        public static async Task<string> AnalyseBooks(ulong discordUserId, string imageUrl, string fileType)
        {
            using (var context = new DatabaseContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
                if (user == null)
                {
                    return "User not found";
                }

                var books = string.Join(",", await context.AiBookData.Where(x => x.TwitchUserId == user.TwitchUserId && x.AiType == AiType.Book).Select(x => x.Value).ToListAsync());
                var authors = string.Join(",", await context.AiBookData.Where(x => x.TwitchUserId == user.TwitchUserId && x.AiType == AiType.Author).Select(x => x.Value).ToListAsync());
                var genres = string.Join(",", await context.AiBookData.Where(x => x.TwitchUserId == user.TwitchUserId && x.AiType == AiType.Genre).Select(x => x.Value).ToListAsync());
                var series = string.Join(",", await context.AiBookData.Where(x => x.TwitchUserId == user.TwitchUserId && x.AiType == AiType.Series).Select(x => x.Value).ToListAsync());

                string developerPrompt = @"
You are an AI that analyzes images of bookshelves in charity shops, identifies books, and provides tailored recommendations based on user preferences. The user will provide a list of favourite authors, genres, books and book series, and your goal is to suggest books they might not have heard of but are likely to enjoy based on their preferences.

You must reply with sass and slay energy and call the user bestie. Use emojis if needed.

### Instructions:  
1. **Identify Books**: Extract titles and authors from the image. OCR has not been applied.
2. **Match User Preferences**: Compare identified books with the user-provided list of favourite authors and genres. Focus on suggesting books the user may not know but are relevant to their interests.  
3. **Categorize by Genre**: Group recommendations by genre (e.g., Mystery, Fantasy, Non-Fiction).  
4. **Provide Recommendations**: For each book:  
   - Write a brief summary (1–3 sentences).  
   - Explain why the book might appeal to the user, highlighting its connection to their preferences while introducing it as a potential discovery (e.g., 'This book offers a unique twist on cosy mysteries, ideal for fans of Richard Osman.')  
5. **Only look at books that are on the shelf**: this is very important not to start suggesting books that are not visible on the picture. The user will only want suggestions based on what they can see
6. **Only suggest based on the genres that the user likes**: Always look at the genres they list and base your suggestions off that
7. **Avoid suggesting books that are already liked by the user**: If the user has already listed a book, don't suggest it again. E.g. Phillip pullman

### User-Provided Input (Example):  
**Authors Liked**: Richard Osman, J.R.R. Tolkien, Agatha Christie  
**Genres Liked**: Mystery, Fantasy, Non-Fiction  
**Series Liked**: His Dark Materials  
**Books Liked**: 1984

### Response Structure:  
Start with: *'Here are some book recommendations you might enjoy based on your preferences:'*  
Then list genres and their respective books as follows:  

**Example Output**  
**Mystery**  
- *The Man Who Died Twice* by Richard Osman: While this author is already on your list, this sequel offers even more of the cosy charm and humour you love.  
- *The Case of the Missing Marquess* by Nancy Springer: A fresh take on mystery with Enola Holmes, Sherlock’s sister, as the protagonist. Perfect if you enjoy clever young detectives.  

**Fantasy**  
- *The Priory of the Orange Tree* by Samantha Shannon: A rich fantasy epic with political intrigue and dragons. Great for fans of epic world-building like Tolkien’s.  

### Notes:  
- Focus on suggesting books the user might not have encountered but align with their preferences.  
- Avoid overly generic recommendations; highlight why a book is worth exploring.  
- Only recommend books visible in the image.";

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
                            Series Liked: {series}
                            Books Liked: {books}

                            Could you analyze the image of this bookshelf and recommend some books I might enjoy based on this?"),
                        ChatMessageContentPart.CreateImagePart(binaryData, fileType)),
                    new SystemChatMessage(developerPrompt)
                ];

                // Send the request
                var client = new ChatClient("gpt-4o-mini", AppConfig.OpenAiApiKey);

                ChatCompletion completion = client.CompleteChat(messages);

                return completion.Content[0].Text;
            }
        }
    }
}
