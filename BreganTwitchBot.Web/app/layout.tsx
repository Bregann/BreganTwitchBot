import '@mantine/core/styles.css'
import '@mantine/notifications/styles.css'
import '@mantine/charts/styles.css'
import { ColorSchemeScript, MantineProvider, mantineHtmlProps } from '@mantine/core'
import { Notifications } from '@mantine/notifications'
import Providers from './providers'
import ClientLayout from '@/components/navigation/ClientLayout'
import { AuthProvider } from '@/context/authContext'
import { theme } from '@/css/theme'
import NextTopLoader from 'nextjs-toploader'
import type { Metadata, Viewport } from 'next'

export const metadata: Metadata = {
  title: {
    default: 'BreganTwitchBot',
    template: '%s | BreganTwitchBot'
  },
  description: 'Points, watchtime, ranks, subathons and giveaways for your Twitch channel and Discord server',
  openGraph: {
    title: 'BreganTwitchBot',
    description: 'Points, watchtime, ranks, subathons and giveaways for your Twitch channel and Discord server',
    type: 'website'
  },
  robots: {
    index: true,
    follow: true
  }
}

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang="en" {...mantineHtmlProps}>
      <head>
        <ColorSchemeScript />
      </head>
      <body>
        <NextTopLoader color="#9146FF" showSpinner={false} />
        <Providers>
          <AuthProvider>
            <MantineProvider defaultColorScheme="dark" theme={theme}>
              <Notifications />
              <ClientLayout>
                {children}
              </ClientLayout>
            </MantineProvider>
          </AuthProvider>
        </Providers>
      </body>
    </html>
  )
}
