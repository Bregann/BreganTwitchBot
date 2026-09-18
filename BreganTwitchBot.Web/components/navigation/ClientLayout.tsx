'use client'

import { usePathname } from 'next/navigation'
import Navigation from './Navigation'
import PublicLayout from './PublicLayout'

interface ClientLayoutProps {
  children: React.ReactNode
}

/**
 * Picks the shell for the current route.
 *
 * The marketing pages get a plain header/footer, everything else gets the app
 * shell with navigation. Login sits on its own with no chrome at all.
 */
export default function ClientLayout({ children }: ClientLayoutProps) {
  const pathname = usePathname()

  const bareRoutes = ['/login', '/auth/callback']

  if (bareRoutes.some(route => pathname.startsWith(route))) {
    return <>{children}</>
  }

  const publicRoutes = ['/', '/about', '/features', '/commands']

  if (publicRoutes.includes(pathname)) {
    return <PublicLayout>{children}</PublicLayout>
  }

  return <Navigation>{children}</Navigation>
}
