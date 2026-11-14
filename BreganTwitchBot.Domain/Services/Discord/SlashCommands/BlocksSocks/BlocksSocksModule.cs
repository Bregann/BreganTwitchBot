using BreganTwitchBot.Domain.Interfaces.Discord;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using SkiaSharp;
using System.Text;
using DiscordColor = Discord.Color;

namespace BreganTwitchBot.Domain.Services.Discord.SlashCommands.BlocksSocks
{
    public class BlocksSocksModule(IDiscordClientProvider discordClient) : InteractionModuleBase<SocketInteractionContext>
    {
        // haven't bothered with DI for this as it's a pretty stupid command and only supporting it because legacy
        // I also just copied this from the old bot as I have no idea how it works lol

        [SlashCommand("socks", "put some beautiful blocks socks on your skin")]
        public async Task GetBlocksSocks([Summary("minecraftign", "Your Minecraft name")] string username)
        {
            await DeferAsync();

            try
            {
                var embedBuilder = new EmbedBuilder();

                string uuid; // 713365310408884236
                var channel = discordClient.Client.GetGuild(270241003468881924).GetChannel(713365310408884236) as IMessageChannel; //blockssocks channel
                try
                {
                    //Create the request
                    var client = new RestClient("https://api.mojang.com/");
                    var request = new RestRequest($"users/profiles/minecraft/{username.ToLower().Trim()}", Method.Get);

                    //Get the response and Deserialize
                    var response = await client.ExecuteAsync(request);

                    if (response.Content == "")
                    {
                        embedBuilder.Title = "That username doesn't exist";
                        embedBuilder.Color = new DiscordColor(255, 17, 0);
                        await FollowupAsync(embed: embedBuilder.Build());
                        return;
                    }

                    dynamic responseDeserialized = JsonConvert.DeserializeObject(response.Content);

                    //add the uuid
                    uuid = responseDeserialized.id;
                }
                catch (Exception e)
                {
                    Log.Fatal($"[BlocksSocks] There has been an error getting the uuid - {e.Message}");
                    embedBuilder.Title = "There was an error getting your uuid. Please try again later";
                    embedBuilder.Color = new DiscordColor(255, 17, 0);
                    await FollowupAsync(embed: embedBuilder.Build());
                    return;
                }
                string base64Data;

                try
                {
                    //Create the request
                    var client = new RestClient("https://sessionserver.mojang.com/");
                    var request = new RestRequest($"session/minecraft/profile/{uuid}", Method.Get);

                    //Get the response and Deserialize
                    var response = await client.ExecuteAsync(request);

                    if (response.Content == "")
                    {
                        embedBuilder.Title = "There was an error getting your profile info. Please try again later";
                        embedBuilder.Color = new DiscordColor(255, 17, 0);
                        await FollowupAsync(embed: embedBuilder.Build());
                        return;
                    }

                    var responseDeserialized = JsonConvert.DeserializeObject<ProfileResponse>(response.Content);

                    if (responseDeserialized == null)
                    {
                        Log.Fatal($"[BlocksSocks] There has been an error getting the profile data");
                        embedBuilder.Title = "There was an error getting your profile info. Make sure you've put in a Minecraft username";
                        embedBuilder.Color = new DiscordColor(255, 17, 0);
                        await FollowupAsync(embed: embedBuilder.Build());
                        return;
                    }

                    //add the uuid
                    base64Data = responseDeserialized.Properties[0].Value;
                }
                catch (Exception e)
                {
                    Log.Fatal($"[BlocksSocks] There has been an error getting the profile data - {e.Message}");
                    embedBuilder.Title = "There was an error getting your profile info. Make sure you've put in a Minecraft username";
                    embedBuilder.Color = new DiscordColor(255, 17, 0);
                    await FollowupAsync(embed: embedBuilder.Build());
                    return;
                }

                //decode the base64
                var data = Convert.FromBase64String(base64Data);
                var decoded = Encoding.ASCII.GetString(data);

                var skinResponse = JsonConvert.DeserializeObject<DecodedResponse>(decoded);

                if (skinResponse == null)
                {
                    embedBuilder.Title = "Error getting skin";
                    embedBuilder.Color = new DiscordColor(255, 17, 0);
                    await FollowupAsync(embed: embedBuilder.Build());
                    return;
                }

                if (skinResponse.Textures.Skin == null)
                {
                    embedBuilder.Title = "That user doesn't have a skin";
                    embedBuilder.Color = new DiscordColor(255, 17, 0);
                    await FollowupAsync(embed: embedBuilder.Build());
                    return;
                }

                using (var httpClient = new HttpClient())
                {
                    var httpResult = await httpClient.GetAsync(skinResponse.Textures.Skin.Url);
                    using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                    using var fileStream = File.Create($"Skins/{uuid}.png");
                    resultStream.CopyTo(fileStream);
                }

                //check if right size
                using (var bmp = SKBitmap.Decode($"Skins/{uuid}.png"))
                {
                    if (bmp == null || bmp.Width != 64 || bmp.Height != 64)
                    {
                        embedBuilder.Title = "Your skin appears not to be 64x64 - this only supports 64x64 skins";
                        embedBuilder.Color = new DiscordColor(255, 17, 0);
                        await FollowupAsync(embed: embedBuilder.Build());
                        return;
                    }

                    using var socks = SKBitmap.Decode("Skins/socks.png");

                    using (var canvas = new SKCanvas(bmp))
                    {
                        //draw socks over the skin
                        using var socksImage = SKImage.FromBitmap(socks);
                        canvas.DrawImage(socksImage, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));

                        //wipe the second-layer left leg area (0,32 16x32) to fully transparent
                        canvas.Save();
                        canvas.ClipRect(SKRect.Create(0, 32, 16, 32));
                        canvas.Clear(SKColors.Transparent);
                        canvas.Restore();
                    }

                    using var image = SKImage.FromBitmap(bmp);
                    using var png = image.Encode(SKEncodedImageFormat.Png, 100);
                    using var outFs = File.Create($"Skins/{uuid}-socks.png");
                    png.SaveTo(outFs);
                }

                using (var socksFs = new FileStream($"Skins/{uuid}-socks.png", FileMode.Open))
                {
                    await channel!.SendFileAsync(socksFs, $"Skins/{uuid}-socks.png");
                    embedBuilder.Title = "Your socks have been added";
                    embedBuilder.Color = new DiscordColor(34, 255, 0);
                    await FollowupAsync(embed: embedBuilder.Build());
                    return;
                }
                ;
            }
            catch (Exception e)
            {
                await FollowupAsync("Oh no there has been an error  - please try again in a min");
                Log.Fatal($"[BlockSocks] Error with the !socks command - {e}");
                return;
            }
        }
    }

    public class ProfileResponse
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("properties")]
        public required List<Property> Properties { get; set; }
    }

    public class Property
    {
        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("value")]
        public required string Value { get; set; }
    }

    public class DecodedResponse
    {
        [JsonProperty("timestamp")]
        public required long Timestamp { get; set; }

        [JsonProperty("profileId")]
        public required string ProfileId { get; set; }

        [JsonProperty("profileName")]
        public required string ProfileName { get; set; }

        [JsonProperty("textures")]
        public required Textures Textures { get; set; }
    }

    public class Textures
    {
        [JsonProperty("SKIN")]
        public required Skin Skin { get; set; }
    }

    public class Skin
    {
        [JsonProperty("url")]
        public required Uri Url { get; set; }
    }

}
