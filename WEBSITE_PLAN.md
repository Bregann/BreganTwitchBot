# BreganTwitchBot website plan

A Next.js front end for the bot, following the conventions already established in [orbit.web](https://github.com/Bregann/Orbit/tree/main/orbit.web).

Three audiences:

1. **Logged out visitors** — a marketing site explaining the bot and its features.
2. **Viewers** — log in with Twitch to see their own stats in the channels they watch.
3. **Broadcasters and their mods** — an admin area for configuring the bot, with per-mod permissions.

---

## Decisions already made

| Decision | Choice | Why |
|---|---|---|
| Framework | Next.js 16, App Router | Matches Orbit |
| UI library | Mantine 9 | Matches Orbit |
| Data fetching | TanStack Query 5 | Matches Orbit |
| Icons | Tabler | Matches Orbit |
| Charts | Mantine Charts / Recharts | Matches Orbit |
| Viewer auth | **Twitch OAuth** | Viewers are already `ChannelUser.TwitchUserId`; no linking step, no passwords |
| URL shape | `/{channel}/subathon` | Channel name at the root, matching the channel-scoped API |
| Project location | `BreganTwitchBot.Web/` | Sits beside the existing projects |

**Everything configurable stays in the database, per channel.** This is the standing rule from the port ([PORTING.md](PORTING.md)) and the website is the reason it exists — these screens are the editor for that config.

---

## Route map

```
/                          Landing page (logged out) / redirect to /me (logged in)
/about                     What the bot does
/features                  Feature breakdown
/commands                  Public command reference
/login                     Twitch OAuth entry point
/auth/callback             OAuth callback handler

/me                        Your stats across every channel you're known in
/me/settings               Your own preferences

/{channel}                 Channel home — live status, recent activity
/{channel}/stats           Stream stats and history
/{channel}/leaderboards    All leaderboards, switchable
/{channel}/subathon        Subathon timer and contributors
/{channel}/commands        That channel's custom commands

/{channel}/admin           Admin dashboard (broadcaster + permitted mods)
/{channel}/admin/settings          Channel config
/{channel}/admin/commands          Custom command editor
/{channel}/admin/ranks             Watchtime ranks
/{channel}/admin/rewards           Channel point rewards
/{channel}/admin/subathon          Subathon rates and control
/{channel}/admin/giveaways         Giveaway config
/{channel}/admin/blacklist         Word blacklist
/{channel}/admin/discord           Discord integration settings
/{channel}/admin/permissions       Who can edit what (broadcaster only)
```

**Route collisions.** `/{channel}` sits at the root, so a channel called `about` or `login` would shadow a real page. Reserved names are rejected at channel registration and the static routes are matched first. Listed here because it's the kind of thing that only bites in production.

---

## Permissions model

The bot has `ChannelUserData.IsSuperMod` but nothing finer grained. The website needs "this mod may edit commands but not the subathon", so:

```
ChannelPermission
  Id
  ChannelId        -> Channel
  ChannelUserId    -> ChannelUser
  Permission       (enum)
  GrantedAt
  GrantedByChannelUserId
```

Permissions as a flags enum:

| Permission | Covers |
|---|---|
| `ViewAdmin` | See the admin area at all |
| `EditCommands` | Custom commands |
| `EditRanks` | Watchtime ranks and rewards |
| `EditSubathon` | Subathon rates, start/stop |
| `EditGiveaways` | Giveaway config |
| `EditBlacklist` | Word blacklist |
| `EditDiscord` | Discord integration |
| `EditChannelConfig` | Currency name, point caps, general config |
| `ManagePermissions` | Grant permissions to others — **broadcaster only, never delegable** |

Rules:

- The broadcaster implicitly has everything.
- `ManagePermissions` cannot be granted; only the broadcaster manages permissions. Stops a mod escalating themselves or locking the broadcaster out.
- Every permission check happens **server side** in the API. The UI hides what you can't use, but hiding is not enforcing.
- Permission changes are audited (who granted what, to whom, when).

---

## Stages

Each stage is independently reviewable. This is one PR overall, so they're commits within it rather than separate PRs.

---

### Stage 1 — Foundations

Get an empty but correct app running.

- `BreganTwitchBot.Web/` scaffolded with the Orbit dependency set.
- `app/layout.tsx` with `MantineProvider`, the dark theme overrides from Orbit, `Notifications`, `NextTopLoader`.
- `app/providers.tsx` — the TanStack Query client, copied from Orbit's server/browser split.
- `helpers/apiClient.ts`, `helpers/QueryKeys.ts`, `helpers/mutations/*` following Orbit's shape.
- `app/api/[...route]/route.ts` — the BFF proxy that forwards to the .NET API and attaches the access token cookie.
- ESLint, TypeScript, Dockerfile, `postcss.config.cjs` matching Orbit.

**Done when:** the app builds, renders a themed empty shell, and a proxied API call reaches the backend.

> ⚠️ **Not verified locally.** `npm install` exits 0 in this environment but doesn't
> materialise packages, so the Stage 1 scaffold has **not been built or typechecked**.
> Run `npm install && npm run build` before trusting it — expect small fixes
> (import paths, Mantine prop names) on the first real build.

---

### Stage 2 — Twitch OAuth

The piece with the most unknowns, so it comes early.

**Backend:**
- `TwitchAuthController`: `GET /api/auth/twitch/login` → redirect to Twitch; `GET /api/auth/twitch/callback` → exchange code, issue tokens.
- Extend `AuthService` to issue JWTs for a Twitch identity rather than only email/password.
- New claims: `twitch_user_id`, `twitch_username`.
- Reuse the existing refresh token flow (`UserRefreshToken`).
- `ChannelUser` rows are matched on `TwitchUserId`; a viewer with no row yet gets a session but empty stats (they've simply never been seen in a tracked channel).

**Frontend:**
- `context/authContext.tsx` adapted from Orbit — same silent-refresh timer, Twitch identity instead of email.
- `proxy.ts` for route protection and token refresh, copied from Orbit's approach.
- `/login` with a single "Continue with Twitch" button.

**Decided:** Twitch OAuth is the only way in. The email/password path has been removed
rather than left as dead code — `User` is now a Twitch identity with no password hash.

**Done when:** you can log in with Twitch, the session survives a refresh, and the API can identify the caller.

**Built.** Backend is done and tested (12 tests). The frontend pieces are written but
unverified — see the Stage 1 note about `npm install`.

---

### Stage 3 — Public site (logged out)

The marketing surface. No auth, no personalisation.

- `/` landing: what the bot is, feature highlights, a "Login with Twitch" call to action.
- `/about`, `/features`, `/commands` — the last reads from the live command registry rather than a hardcoded list, so it can't go stale.
- Public layout distinct from the app shell: simple header, footer, no sidebar.
- Responsive and dark-mode correct throughout.

**Backend:** a public endpoint listing commands from the `TwitchCommand` attribute registry.

**Done when:** a logged-out visitor can understand what the bot does and how to get it.

**Built.** `/`, `/features`, `/about` and `/commands`, with the command list read from
the live registry via `GET /api/Public/Commands`.

---

### Stage 4 — Channel pages (public)

The read-only channel surface. Mostly served by the API controllers from #54.

- `/{channel}` — live status, current game/title, recent activity.
- `/{channel}/leaderboards` — all types, switchable, paginated.
- `/{channel}/subathon` — **live countdown**, total time, top contributors.
- `/{channel}/stats` — stream history with charts (viewers over time, follower growth, messages).
- `/{channel}/commands` — that channel's custom commands.
- Channel layout with its own sub-navigation and a channel switcher.

**Backend:** extend #54's controllers — stream history, channel summary, live status.

**Subathon timer note:** the API returns `SecondsLeft` at request time. The page counts down client side and re-syncs periodically rather than polling every second.

**Done when:** a viewer can browse a channel's public data without logging in.

**Built.** Overview, leaderboards, subathon (with a client side countdown that resyncs
every 30s), stats with charts, and commands. Backed by the #54 controllers plus two new
channel endpoints.

---

### Stage 5 — Viewer stats

What a logged-in viewer sees about themselves.

- `/me` — every channel they're known in, with points, watchtime, rank, streaks, gambling record, position on each leaderboard.
- `/me/settings` — their own preferences (level-up notifications and so on).
- Per-channel breakdown cards, progress toward the next rank.

**Backend:** `GET /api/me/stats` returning the caller's stats across channels, resolved from their Twitch id claim.

**Done when:** logging in shows your real numbers with no linking step.

**Built.** `GET /api/Me/Stats` and the `/me` page, with rank progress bars. Someone signed
in whom the bot has never seen gets an empty list rather than an error, since that's a real
state rather than a failure.

---

### Stage 6 — Permissions

Build before the admin screens, since everything after depends on it.

**Backend:**
- `ChannelPermission` model + migration.
- `IPermissionService` — grant, revoke, check.
- An authorisation attribute/handler so endpoints declare what they need.
- Audit logging of grants and revocations.

**Frontend:**
- `/{channel}/admin/permissions` — broadcaster only. List mods, toggle permissions, see who granted what.
- A `usePermissions` hook and a guard component for conditional UI.

**Done when:** the broadcaster can grant a mod exactly one permission and that mod sees exactly that area — verified server side, not just hidden.

---

### Stage 7 — Admin

The configuration screens. Each maps to config that currently has no editor.

| Page | Edits | Permission |
|---|---|---|
| `/admin/settings` | Currency name, point cap, general config | `EditChannelConfig` |
| `/admin/commands` | Custom commands — add, edit, delete | `EditCommands` |
| `/admin/ranks` | Watchtime ranks, minutes, Discord roles | `EditRanks` |
| `/admin/rewards` | Channel point rewards (from #48) | `EditRanks` |
| `/admin/subathon` | Rate bands (#49), start/stop, manual time | `EditSubathon` |
| `/admin/giveaways` | Entry weighting (#50), run giveaways | `EditGiveaways` |
| `/admin/blacklist` | Banned/timeout/warn words | `EditBlacklist` |
| `/admin/discord` | Channel ids, roles, self-assign roles (#58), monthly leaderboard roles (#56) | `EditDiscord` |

**Backend:** write endpoints for each, all permission gated. This is the largest backend chunk — the port built read paths; these are writes.

**Done when:** a broadcaster can configure the bot entirely from the web, with no SQL.

---

### Stage 8 — Polish

- Loading skeletons rather than spinners.
- Error boundaries with useful messages.
- Empty states (no subathon, no stats yet, channel not found).
- Mobile pass over every page.
- SEO metadata, Open Graph images for channel pages.
- Accessibility pass: keyboard navigation, contrast, screen reader labels.

---

## Backend work summary

The port built read paths; the website needs writes and auth.

| Area | Work |
|---|---|
| Twitch OAuth | New controller, JWT claims for Twitch identity |
| Permissions | New model, service, migration, authorisation handler |
| Viewer stats | `GET /api/me/stats` |
| Channel read | Extend #54 — stream history, live status, channel summary |
| Admin writes | Roughly 8 controllers, all permission gated |
| Public | Command registry endpoint |

---

## Things worth settling before Stage 2

1. ~~Keep email/password auth, or Twitch-only?~~ **Twitch only** — done in stage 2.
2. **Who can register a channel?** Self-serve via Twitch login, or manual for now?
3. **Hosting** — same box as the bot, or separate? Affects the proxy's `API_BASE_URL` and CORS.
4. **Domain** — the old bot referenced `bot.bregan.me`. Same, or new?
5. **Does the landing page need branding** (logo, colours) or is Mantine's default theme fine to start?

---

## Deliberately out of scope

- Real-time updates (websockets/SSE). Polling and client-side countdowns are enough initially; revisit if the subathon page feels stale.
- Public user profiles. `/me` is private to you.
- Editing anything from the website that the bot writes at runtime (points, watchtime). Read-only to avoid fighting the bot for the same rows.
