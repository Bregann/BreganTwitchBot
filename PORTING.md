# Porting progress

Tracks what still needs bringing over from `bot old/` into the current bot.

**One feature per PR.** Each PR branches off `main` once [#42](https://github.com/Bregann/BreganTwitchBot/pull/42) (single bot connection) is merged — until then, branch off `refactor/single-twitch-bot-connection`.

Status: ⬜ not started · 🟡 in progress · ✅ done · ❌ dropped (won't port)

---

## Twitch features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Stream stats tracking | ⬜ | — | `Data/TwitchBot/StreamStats.cs` (440 lines) | Biggest item. `TwitchStreamStats` + `StreamViewCount` models already exist, no service. Aggregates per-stream counters (bits, subs, follows, messages, commands, gambling, bans/timeouts, unique viewers) and writes on stream end. Likely worth splitting into 2 PRs: collection, then persistence/reporting. |
| Subathon | ⬜ | — | `Data/TwitchBot/Subathon.cs` (340), `Commands/Subathon/Subathon.cs` (54) | `Subathon` model exists but is a stub — only `ChannelId`/`ChannelUserId`, no timer/state fields, so needs a schema migration. Bits/subs/gifted subs add time on a sliding scale that tapers as total hours climb. |
| Channel points redemptions | ⬜ | — | `Data/TwitchBot/Events/ChannelPoints.cs` (36) | `OnCustomRewardRedeemed` in `WebsocketHostedService.cs:254` currently only logs. Old bot granted points/actions per redemption. |
| `!uptime` | ⬜ | — | `Commands/Uptime/Uptime.cs` (67) | Stream uptime via Helix `GetStreamsAsync`. Old version had a 5s cooldown with a supermod bypass. |
| `!title` / `!game` | ⬜ | — | `Commands/StreamInfo/Title.cs` (84), `Game.cs` (93) | Read **and** set (set requires mod). Needs broadcaster token for the update. |
| `!followers` / `!subs` | ⬜ | — | `Commands/StreamInfo/Followers.cs` (32), `Subs.cs` (34) | Counts via Helix. Subs needs the broadcaster token. |
| `!addmarbleswin` | ⬜ | — | `Commands/Marbles/Marbles.cs` (34) | `ChannelUserStats.MarblesWins` column already exists — just the command + increment. Small; good warm-up PR. |

## Discord features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Giveaways | ⬜ | — | `SlashCommands/Data/Giveaway/Giveaway.cs` (151) | `ChannelConfig.DiscordGiveawayChannelId` already exists. Button-based entry, so needs the button interaction path wired up. |
| `/whois` | ⬜ | — | `SlashCommands/Data/GeneralCommands/Whois/Whois.cs` | Shows linked Twitch/Discord identity + stats. Pairs with existing linking service. |
| Hours/points lookup | ⬜ | — | `SlashCommands/Data/HoursPoints/HoursPoints.cs` (200) | Check overlap with the existing `Daily`/`Levelling` modules before porting — may be partly covered. |
| Discord leaderboards | ⬜ | — | `SlashCommands/Data/Leaderboards/DiscordLeaderboards.cs` (54), `Leaderboards.cs` (127) | Twitch-side leaderboards are already ported; this is the Discord surface for them. |

## API / controllers

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Leaderboards endpoints | ⬜ | — | `Controllers/LeaderboardsController.cs`, `Data/Api/LeaderboardsData.cs` (133) | Only `ExampleController` exists in the new bot. Confirm the frontend contract before porting. |
| Subathon endpoints | ⬜ | — | `Controllers/SubathonController.cs`, `Data/Api/SubathonData.cs` (83) | Depends on the Subathon port landing first. |
| Commands endpoint | ⬜ | — | `Controllers/CommandsController.cs` | Lists available commands. Should read from the `TwitchCommand` attribute registry rather than a hardcoded list. |

---

## Already ported (no action)

Points/daily/weekly/monthly/yearly · hours & watchtime · gambling & spins · linking · leaderboards (Twitch) · custom commands · word blacklist · 8ball · dad jokes · follow age · Twitch bosses · Discord levelling · Discord daily · book recs · blocks/socks

## Decisions

- **Per-channel, not global.** The old bot was single-channel with static `AppConfig` state; everything ported must be keyed by channel and use DI, not statics.
- **Broadcaster vs bot token.** Subs, title/game updates and channel point reads need the channel's own broadcaster token (`GetBroadcasterApiClientFromChannelName`). Chat and moderation use the shared bot account (`GetBotApiClient`). See the class comment on `TwitchApiConnection`.
- **Tests.** Follow the existing `BreganTwitchBot.DomainTests` pattern (Testcontainers + Postgres) for anything with data logic.

## Open questions

- [ ] Does the subathon still need the tapering time scale, or has the format changed?
- [ ] Are the API controllers still consumed by a frontend, or can they be dropped?
- [ ] Is `!addmarbleswin` still wanted, given marbles is a separate bot?
- [ ] Any of these worth deliberately **not** porting?
