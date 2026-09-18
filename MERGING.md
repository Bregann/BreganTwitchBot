# Merge guide

17 open PRs, built in parallel. Each one merges cleanly into `main` on its own, but several
touch the same files, so the order matters. This is the order to merge them in and what to
expect at each step.

Checked with `git merge-tree` against `origin/main`: **no PR conflicts with `main` today**.
The conflicts are between the PRs themselves.

---

## The short version

Merge in this order. After each merge, the next PR may show a conflict in one of the shared
files below — take **both** sides (they're additive) and carry on.

| Order | PR | Why here |
|---|---|---|
| 1 | [#44](https://github.com/Bregann/BreganTwitchBot/pull/44) marbles | Self-contained |
| 2 | [#45](https://github.com/Bregann/BreganTwitchBot/pull/45) uptime | Introduces `DurationFormatHelper` |
| 3 | [#46](https://github.com/Bregann/BreganTwitchBot/pull/46) followers/subs | Introduces `CommandCooldownHelper` |
| 4 | [#47](https://github.com/Bregann/BreganTwitchBot/pull/47) title/game | Stacked on #46 — merge straight after |
| 5 | [#51](https://github.com/Bregann/BreganTwitchBot/pull/51) whois | Self-contained |
| 6 | [#52](https://github.com/Bregann/BreganTwitchBot/pull/52) hours/points | Self-contained |
| 7 | [#53](https://github.com/Bregann/BreganTwitchBot/pull/53) Discord leaderboards | Needed by #54 |
| 8 | [#48](https://github.com/Bregann/BreganTwitchBot/pull/48) channel points | **Migration** |
| 9 | [#49](https://github.com/Bregann/BreganTwitchBot/pull/49) subathon | **Migration** |
| 10 | [#50](https://github.com/Bregann/BreganTwitchBot/pull/50) giveaways | **Migration** |
| 11 | [#54](https://github.com/Bregann/BreganTwitchBot/pull/54) API controllers | Needs #49 and #53 |
| 12 | [#55](https://github.com/Bregann/BreganTwitchBot/pull/55) stream stats | Needs #46 |
| 13 | [#56](https://github.com/Bregann/BreganTwitchBot/pull/56) monthly roles + summary | **Migration**, needs #55 |
| 14 | [#57](https://github.com/Bregann/BreganTwitchBot/pull/57) scheduled jobs | Needs #55 |
| 15 | [#58](https://github.com/Bregann/BreganTwitchBot/pull/58) self assign roles | **Migration** |
| 16 | [#43](https://github.com/Bregann/BreganTwitchBot/pull/43) porting tracker | Takes the final `PORTING.md` |
| 17 | [#59](https://github.com/Bregann/BreganTwitchBot/pull/59) **delete `bot old/`** | **Must be last** |
| — | [#60](https://github.com/Bregann/BreganTwitchBot/pull/60) website | Independent, merge whenever |

---

## Why the order

**Migrations must merge in timestamp order.** EF applies them by filename timestamp and each
carries a model snapshot. Merging them out of order leaves the snapshot describing a state the
migrations don't produce, and the next `migrations add` generates nonsense.

```
20260918105920_AddChannelPointRewards      #48
20260918112220_SubathonPerChannel          #49
20260918114058_AddDiscordGiveaways         #50
20260918121540_AddMonthlyLeaderboardRoles  #56
20260918122702_AddDiscordSelfAssignRoles   #58
```

That's #48 → #49 → #50 → #56 → #58, which the table respects.

**#59 deletes `bot old/`.** Merging it before #56, #57 and #58 loses the source those were
ported from, and they're the last three features found in the file-by-file audit. It genuinely
has to go last.

**#47 is stacked on #46** rather than `main`, because both are StreamInfo commands and #47
uses the `CommandCooldownHelper` #46 introduces.

---

## Expected conflicts, and what to do

These files are touched by several PRs. Every conflict in them is **additive** — two PRs adding
different lines in the same place. Take both sides.

| File | PRs | What the conflict looks like |
|---|---|---|
| `PORTING.md` | 13 | Each PR marks its own row ✅. Take whichever is more complete; #43 last wins anyway. |
| `Program.cs` | 13 | Each adds its own DI registration lines. Keep all of them. |
| `AppDbContext.cs` | 6 | Each adds a `DbSet`. Keep all. |
| `Channel.cs` | 6 | Each adds a navigation collection. Keep all. |
| `PostgresqlContextModelSnapshot.cs` | 6 | **Don't hand-merge this.** See below. |
| `TwitchEventHandlerService.cs` | 5 | Different handlers, or different lines in the same handler. Keep both. |
| `TwitchApiInteractionService.cs` | 4 | Each adds API methods. Keep all. |
| `DurationFormatHelper.cs` | 4 | **Byte-identical** in every PR — take either side. |
| `CommandCooldownHelper.cs` | 2 | Byte-identical — take either side. |
| `StreamStatsService.cs` | 3 | #55 creates it; #56 and #57 add methods. Keep all. |

### The model snapshot

`PostgresqlContextModelSnapshot.cs` is generated. If it conflicts, don't resolve it by hand —
take either side, then regenerate:

```bash
dotnet ef migrations remove --project BreganTwitchBot.Domain --startup-project BreganTwitchBot --context PostgresqlContext
# then re-add the migration that was removed, or:
dotnet ef migrations add SnapshotFix --project BreganTwitchBot.Domain --startup-project BreganTwitchBot --context PostgresqlContext --output-dir Database/Migrations/Postgresql
```

If the regenerated migration comes out empty, the snapshot was already correct — delete it.

### Duplicated helper files

Several PRs include a byte-identical copy of a helper from another PR, so each one builds
standalone. Once the first is merged the rest become no-ops. That was deliberate; nothing to
clean up.

---

## After merging

1. `dotnet build BreganTwitchBot.sln` — should be clean.
2. `dotnet test BreganTwitchBot.DomainTests` — should be green.
3. `dotnet ef database update` to apply the five migrations.

### Data you need to insert yourself

Nothing below is seeded, and the matching feature stays silent until it exists:

| Table | Why | From |
|---|---|---|
| `EnvironmentalSettings` — the four `BotTwitchChannel*` rows | **The bot will not connect without these.** Also needed before first boot or refreshed tokens won't persist. | #42 (merged) |
| `ChannelPointRewards` | Your "goose" reward and any others | #48 |
| `MonthlyLeaderboardRoles` | Role ids for the monthly bits/subs roles | #56 |
| `DiscordSelfAssignRoles` | The twelve self-assign roles | #58 |
| `SubathonRates` | **Auto-seeded** with the old bot's values by the migration | #49 |

---

## Not ported, on purpose

One thing from the old bot was deliberately left out: the `AddSubathonTime` POST endpoint,
which took a shared secret in the URL path. Reasoning is in #54. The project has JWT auth if
you want an authenticated version — say the word and it's a small addition.
