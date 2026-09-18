'use client'

import { AppShell, Burger, Group, NavLink, ScrollArea, Text } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconChartBar, IconHome, IconUser } from '@tabler/icons-react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'

/**
 * The app shell used once past the marketing pages.
 *
 * Channel aware navigation lands in stage 4 when there are channel pages to
 * navigate to - for now this is the frame and the viewer's own links.
 */
export default function Navigation({ children }: { children: React.ReactNode }) {
  const [opened, { toggle }] = useDisclosure()
  const pathname = usePathname()

  const navItems = [
    { label: 'Home', href: '/', icon: IconHome },
    { label: 'My stats', href: '/me', icon: IconUser },
  ]

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 260, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md" justify="space-between">
          <Group gap="xs">
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Text component={Link} href="/" fw={700} style={{ textDecoration: 'none' }} c="bright">
              BreganTwitchBot
            </Text>
          </Group>
          <IconChartBar size={20} />
        </Group>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <AppShell.Section grow component={ScrollArea}>
          {navItems.map(item => (
            <NavLink
              key={item.href}
              component={Link}
              href={item.href}
              label={item.label}
              leftSection={<item.icon size={18} />}
              active={pathname === item.href}
            />
          ))}
        </AppShell.Section>
      </AppShell.Navbar>

      <AppShell.Main>{children}</AppShell.Main>
    </AppShell>
  )
}
