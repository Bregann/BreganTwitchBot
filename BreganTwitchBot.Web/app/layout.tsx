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
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: {
    default: 'BreganTwitchBot',
    template: '%s | BreganTwitchBot'
  },
  description: 'Points, watchtime, subathons and more for your Twitch channel'
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
