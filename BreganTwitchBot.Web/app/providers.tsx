'use client'

import {
  isServer,
  QueryClient,
  QueryClientProvider,
} from '@tanstack/react-query'

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        // with SSR we want a stale time above 0 so the client doesn't refetch
        // everything again immediately on hydration
        staleTime: 1000 * 60 * 5,
        refetchOnWindowFocus: false
      },
    },
  })
}

let browserQueryClient: QueryClient | undefined = undefined

function getQueryClient() {
  if (isServer) {
    // server: always a new client so requests can't share cache between users
    return makeQueryClient()
  }

  // browser: reuse the client so a suspending render doesn't throw it away
  if (browserQueryClient === undefined) {
    browserQueryClient = makeQueryClient()
  }

  return browserQueryClient
}

export default function Providers({ children }: { children: React.ReactNode }) {
  const queryClient = getQueryClient()

  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}
