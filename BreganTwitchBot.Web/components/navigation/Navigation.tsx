'use client'

import { useAuth } from '@/context/authContext'
import {
  Avatar,
  AppShell,
  Burger,
  Button,
  Group,
  Menu,
  NavLink,
  ScrollArea,
  Text
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import {
  IconBrandTwitch,
  IconChevronDown,
  IconHome,
  IconLogout,
  IconSettings,
  IconUser
} from '@tabler/icons-react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'

/**
 * The app shell used once past the marketing pages.
 *
 * Channels the signed in user broadcasts get an admin link, since that's the only
 * place the admin area is reachable from.
 */
export default function Navigation({ children }: { children: React.ReactNode }) {
  const [opened, { toggle, close }] = useDisclosure()
  const pathname = usePathname()
  const { isAuthenticated, user, logout } = useAuth()

  const navItems = [
    { label: 'Home', href: '/', icon: IconHome },
    { label: 'My stats', href: '/me', icon: IconUser },
  ]

  const myChannels = user?.broadcasterOfChannels ?? []

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

          {isAuthenticated
            ? (
              <Menu position="bottom-end">
                <Menu.Target>
                  <Button variant="subtle" rightSection={<IconChevronDown size={14} />} px="xs">
                    <Group gap="xs">
                      <Avatar src={user?.profileImageUrl} size={24} radius="xl" />
                      <Text size="sm" visibleFrom="xs">{user?.twitchDisplayName ?? user?.twitchUsername}</Text>
                    </Group>
                  </Button>
                </Menu.Target>

                <Menu.Dropdown>
                  <Menu.Item component={Link} href="/me" leftSection={<IconUser size={14} />}>
                    My stats
                  </Menu.Item>
                  <Menu.Divider />
                  <Menu.Item color="red" leftSection={<IconLogout size={14} />} onClick={() => void logout()}>
                    Log out
                  </Menu.Item>
                </Menu.Dropdown>
              </Menu>
            )
            : (
              <Button component={Link} href="/login" size="sm" leftSection={<IconBrandTwitch size={16} />}>
                Login
              </Button>
            )}
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
              onClick={close}
            />
          ))}

          {myChannels.length > 0 && (
            <>
              <Text size="xs" c="dimmed" tt="uppercase" fw={700} mt="md" mb="xs" px="sm">
                Your channels
              </Text>

              {myChannels.map(channelName => (
                <NavLink
                  key={channelName}
                  label={channelName}
                  leftSection={<IconBrandTwitch size={18} />}
                  defaultOpened={pathname.startsWith(`/${channelName}`)}
                >
                  <NavLink
                    component={Link}
                    href={`/${channelName}`}
                    label="Channel page"
                    active={pathname === `/${channelName}`}
                    onClick={close}
                  />
                  <NavLink
                    component={Link}
                    href={`/${channelName}/admin`}
                    label="Admin"
                    leftSection={<IconSettings size={16} />}
                    active={pathname.startsWith(`/${channelName}/admin`)}
                    onClick={close}
                  />
                </NavLink>
              ))}
            </>
          )}
        </AppShell.Section>
      </AppShell.Navbar>

      <AppShell.Main>{children}</AppShell.Main>
    </AppShell>
  )
}
