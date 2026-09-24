# BreganTwitchBot.Web

The Next.js front end for the bot. See [WEBSITE_PLAN.md](../WEBSITE_PLAN.md) for the staged plan.

## Running

```bash
npm install
npm run dev
```

The app expects the .NET API at `https://localhost:7248/api` in development. Set
`API_BASE_URL` for other environments.

## Conventions

Follows [orbit.web](https://github.com/Bregann/Orbit/tree/main/orbit.web):

- **Mantine** for UI, with the dark palette override and Twitch purple as the primary colour.
- **TanStack Query** for server state, with query keys in `helpers/QueryKeys.ts`.
- **`helpers/apiClient.ts`** wraps fetch, handles 401 refresh and retries.
- **`app/api/[...route]/route.ts`** proxies to the .NET API and attaches the access
  token from its httpOnly cookie, so the token is never exposed to client scripts.
