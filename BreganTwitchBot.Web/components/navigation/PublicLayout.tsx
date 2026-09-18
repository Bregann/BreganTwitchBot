'use client'

import { Anchor, AppShell, Burger, Button, Container, Group, Stack, Text } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconBrandTwitch } from '@tabler/icons-react'
import Link from 'next/link'

const links = [
  { label: 'Home', href: '/' },
  { label: 'Features', href: '/features' },
  { label: 'Commands', href: '/commands' },
  { label: 'About', href: '/about' },
]

/**
 * Shell for the logged out marketing pages - header and footer, no sidebar.
 */
export default function PublicLayout({ children }: { children: React.ReactNode }) {
  const [opened, { toggle, close }] = useDisclosure(false)

  return (
    <AppShell
      header={{ height: 60 }}
      navbar={{ width: 240, breakpoint: 'sm', collapsed: { desktop: true, mobile: !opened } }}
      padding={0}
    >
      <AppShell.Header>
        <Container size="lg" h="100%">
          <Group h="100%" justify="space-between">
            <Group gap="xs">
              <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
              <Anchor component={Link} href="/" underline="never" fw={700} c="bright">
                BreganTwitchBot
              </Anchor>
            </Group>

            <Group gap="lg" visibleFrom="sm">
              {links.map(link => (
                <Anchor key={link.href} component={Link} href={link.href} size="sm" c="dimmed" underline="hover">
                  {link.label}
                </Anchor>
              ))}
            </Group>

            <Button
              component={Link}
              href="/login"
              leftSection={<IconBrandTwitch size={16} />}
              size="sm"
            >
              Login
            </Button>
          </Group>
        </Container>
      </AppShell.Header>

      <AppShell.Navbar p="md">
        <Stack gap="xs">
          {links.map(link => (
            <Anchor key={link.href} component={Link} href={link.href} onClick={close} size="sm">
              {link.label}
            </Anchor>
          ))}
        </Stack>
      </AppShell.Navbar>

      <AppShell.Main>
        {children}

        <Container size="lg" py="xl">
          <Group justify="space-between" pt="xl" mt="xl" style={{ borderTop: '1px solid var(--mantine-color-default-border)' }}>
            <Text size="sm" c="dimmed">BreganTwitchBot</Text>
            <Anchor href="https://github.com/Bregann/BreganTwitchBot" target="_blank" size="sm" c="dimmed">
              GitHub
            </Anchor>
          </Group>
        </Container>
      </AppShell.Main>
    </AppShell>
  )
}
