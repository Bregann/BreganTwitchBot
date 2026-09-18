# Porting progress

Tracks what still needs bringing over from `bot old/` into the current bot.

**One feature per PR.** Each PR branches off `main` once [#42](https://github.com/Bregann/BreganTwitchBot/pull/42) (single bot connection) is merged — until then, branch off `refactor/single-twitch-bot-connection`.

Status: ⬜ not started · 🟡 in progress · ✅ done · ❌ dropped (won't port)

---

## Twitch features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Stream stats tracking | ⬜ | — | `Data/TwitchBot/StreamStats.cs` (440 lines) | Biggest item. `TwitchStreamStats` + `StreamViewCount` models already exist, no service. Aggregates per-stream counters (bits, subs, follows, messages, commands, gambling, bans/timeouts, unique viewers) and writes on stream end. Likely worth splitting into 2 PRs: collection, then persistence/reporting. |
| Subathon | ✅ | [#49](https://github.com/Bregann/BreganTwitchBot/pull/49) | `Data/TwitchBot/Subathon.cs` (340), `Commands/Subathon/Subathon.cs` (54) | Done. Taper now a per-channel `SubathonRates` table. |
| Channel points redemptions | ✅ | [#48](https://github.com/Bregann/BreganTwitchBot/pull/48) | `Data/TwitchBot/Events/ChannelPoints.cs` (36) | Done. New `ChannelPointRewards` table. |
| `!uptime` | ✅ | [#45](https://github.com/Bregann/BreganTwitchBot/pull/45) | `Commands/Uptime/Uptime.cs` (67) | Done. Added `DurationFormatHelper`. |
| `!title` / `!game` | ✅ | [#47](https://github.com/Bregann/BreganTwitchBot/pull/47) | `Commands/StreamInfo/Title.cs` (84), `Game.cs` (93) | Done. Bare reads, args sets (mods only). |
| `!followers` / `!subs` | ✅ | [#46](https://github.com/Bregann/BreganTwitchBot/pull/46) | `Commands/StreamInfo/Followers.cs` (32), `Subs.cs` (34) | Done. Added `CommandCooldownHelper`. |
| `!addmarbleswin` | ✅ | [#44](https://github.com/Bregann/BreganTwitchBot/pull/44) | `Commands/Marbles/Marbles.cs` (34) | Done. |

## Discord features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Giveaways | ✅ | [#50](https://github.com/Bregann/BreganTwitchBot/pull/50) | `SlashCommands/Data/Giveaway/Giveaway.cs` (151) | Done. Explicit entry requirements replace silent rigging. |
| `/whois` | ✅ | [#51](https://github.com/Bregann/BreganTwitchBot/pull/51) | `SlashCommands/Data/GeneralCommands/Whois/Whois.cs` | Done. Fixed an index crash and a guaranteed NRE. |
| Hours/points lookup | ✅ | [#52](https://github.com/Bregann/BreganTwitchBot/pull/52) | `SlashCommands/Data/HoursPoints/HoursPoints.cs` (200) | Done: `/hours`, `/points`, `/prestige`. |
| Discord leaderboards | ✅ | [#53](https://github.com/Bregann/BreganTwitchBot/pull/53) | `SlashCommands/Data/Leaderboards/DiscordLeaderboards.cs` (54), `Leaderboards.cs` (127) | Done. 9 commands. Fixed monthly hours ordering by bits. |

## API / controllers

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Leaderboards endpoints | ✅ | [#54](https://github.com/Bregann/BreganTwitchBot/pull/54) | `Controllers/LeaderboardsController.cs`, `Data/Api/LeaderboardsData.cs` (133) | Done. Channel scoped, shares the Discord leaderboard queries. |
| Subathon endpoints | ✅ | [#49](https://github.com/Bregann/BreganTwitchBot/pull/49) | `Controllers/SubathonController.cs`, `Data/Api/SubathonData.cs` (83) | Done. Taper now a per-channel `SubathonRates` table. |
| Commands endpoint | ✅ | [#54](https://github.com/Bregann/BreganTwitchBot/pull/54) | `Controllers/CommandsController.cs` | Done. Custom commands per channel. |

---

## Already ported (no action)

Points/daily/weekly/monthly/yearly · hours & watchtime · gambling & spins · linking · leaderboards (Twitch) · custom commands · word blacklist · 8ball · dad jokes · follow age · Twitch bosses · Discord levelling · Discord daily · book recs · blocks/socks

## Decisions

- **Anything configurable lives in the database, per channel.** The website will eventually edit these, so no tunables as constants or appsettings.
- **No silently rigged or fake behaviour.** Where the old bot quietly excluded users or invented answers, the new version states the truth instead.
- **Per-channel, not global.** The old bot was single-channel with static `AppConfig` state; everything ported must be keyed by channel and use DI, not statics.
- **Broadcaster vs bot token.** Subs, title/game updates and channel point reads need the channel's own broadcaster token (`GetBroadcasterApiClientFromChannelName`). Chat and moderation use the shared bot account (`GetBotApiClient`). See the class comment on `TwitchApiConnection`.
- **Tests.** Follow the existing `BreganTwitchBot.DomainTests` pattern (Testcontainers + Postgres) for anything with data logic.

## Open questions

- [x] Subathon taper kept, in a per-channel `SubathonRates` table.
- [x] A website is planned (designed separately), so the API controllers stay in scope.
- [x] `!addmarbleswin` ported.
- [ ] Any of these worth deliberately **not** porting?
