# Porting progress

Tracks what still needs bringing over from `bot old/` into the current bot.

**The port is complete and `bot old/` has been removed ([#59](https://github.com/Bregann/BreganTwitchBot/pull/59)).** This file is kept as the record of what moved and why.

**One feature per PR.** Each PR branches off `main` once [#42](https://github.com/Bregann/BreganTwitchBot/pull/42) (single bot connection) is merged — until then, branch off `refactor/single-twitch-bot-connection`.

Status: ⬜ not started · 🟡 in progress · ✅ done · ❌ dropped (won't port)

---

## Twitch features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Stream stats tracking | ✅ | [#55](https://github.com/Bregann/BreganTwitchBot/pull/55) | `Data/TwitchBot/StreamStats.cs` (440 lines) | Done. |
| Subathon | ✅ | [#49](https://github.com/Bregann/BreganTwitchBot/pull/49) | `Data/TwitchBot/Subathon.cs` (340), `Commands/Subathon/Subathon.cs` (54) | Done. |
| Channel points redemptions | ✅ | [#48](https://github.com/Bregann/BreganTwitchBot/pull/48) | `Data/TwitchBot/Events/ChannelPoints.cs` (36) | Done. |
| `!uptime` | ✅ | [#45](https://github.com/Bregann/BreganTwitchBot/pull/45) | `Commands/Uptime/Uptime.cs` (67) | Done. |
| `!title` / `!game` | ✅ | [#47](https://github.com/Bregann/BreganTwitchBot/pull/47) | `Commands/StreamInfo/Title.cs` (84), `Game.cs` (93) | Done. |
| `!followers` / `!subs` | ✅ | [#46](https://github.com/Bregann/BreganTwitchBot/pull/46) | `Commands/StreamInfo/Followers.cs` (32), `Subs.cs` (34) | Done. |
| `!addmarbleswin` | ✅ | [#44](https://github.com/Bregann/BreganTwitchBot/pull/44) | `Commands/Marbles/Marbles.cs` (34) | Done. |

## Discord features

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Giveaways | ✅ | [#50](https://github.com/Bregann/BreganTwitchBot/pull/50) | `SlashCommands/Data/Giveaway/Giveaway.cs` (151) | Done. |
| `/whois` | ✅ | [#51](https://github.com/Bregann/BreganTwitchBot/pull/51) | `SlashCommands/Data/GeneralCommands/Whois/Whois.cs` | Done. |
| Hours/points lookup | ✅ | [#52](https://github.com/Bregann/BreganTwitchBot/pull/52) | `SlashCommands/Data/HoursPoints/HoursPoints.cs` (200) | Done. |
| Discord leaderboards | ✅ | [#53](https://github.com/Bregann/BreganTwitchBot/pull/53) | `SlashCommands/Data/Leaderboards/DiscordLeaderboards.cs` (54), `Leaderboards.cs` (127) | Done. |

## API / controllers

| Feature | Status | PR | Old source | Notes |
|---|---|---|---|---|
| Leaderboards endpoints | ✅ | [#54](https://github.com/Bregann/BreganTwitchBot/pull/54) | `Controllers/LeaderboardsController.cs`, `Data/Api/LeaderboardsData.cs` (133) | Done. |
| Subathon endpoints | ✅ | [#49](https://github.com/Bregann/BreganTwitchBot/pull/49) | `Controllers/SubathonController.cs`, `Data/Api/SubathonData.cs` (83) | Done. |
| Commands endpoint | ✅ | [#54](https://github.com/Bregann/BreganTwitchBot/pull/54) | `Controllers/CommandsController.cs` | Done. |

---

## Found in the file-by-file audit

The original survey was a sweep, not an exhaustive audit. Going through all 42 source files before deleting them turned up four more features:

| Feature | Status | PR | Notes |
|---|---|---|---|
| Monthly bits/subs leaderboard roles | ✅ | [#56](https://github.com/Bregann/BreganTwitchBot/pull/56) | Role names were hardcoded; the monthly total columns existed but nothing wrote to them. |
| End of stream Discord summary | ✅ | [#56](https://github.com/Bregann/BreganTwitchBot/pull/56) | Posted to the event channel when a stream ends. |
| Viewer sampling, follower check, member count jobs | ✅ | [#57](https://github.com/Bregann/BreganTwitchBot/pull/57) | Without viewer sampling, average and peak viewers would always have been 0. |
| Self assign role buttons | ✅ | [#58](https://github.com/Bregann/BreganTwitchBot/pull/58) | Twelve hardcoded roles become per-channel config. |

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
- [x] Only the old `AddSubathonTime` POST endpoint (shared secret in the URL path) — see #54.
