'use client'

import { Card, NavLink, Grid, Stack, Title } from '@mantine/core'
import {
  IconCoin,
  IconGift,
  IconMessage,
  IconSettings,
  IconShield,
  IconStairsUp,
  IconClock,
  IconBrandDiscord,
  IconBan
} from '@tabler/icons-react'
import Link from 'next/link'
import { usePathname, useParams } from 'next/navigation'

const sections = [
  { label: 'Settings', path: '/settings', icon: IconSettings },
  { label: 'Commands', path: '/commands', icon: IconMessage },
  { label: 'Ranks', path: '/ranks', icon: IconStairsUp },
  { label: 'Rewards', path: '/rewards', icon: IconCoin },
  { label: 'Subathon', path: '/subathon', icon: IconClock },
  { label: 'Giveaways', path: '/giveaways', icon: IconGift },
  { label: 'Blacklist', path: '/blacklist', icon: IconBan },
  { label: 'Discord', path: '/discord', icon: IconBrandDiscord },
  { label: 'Permissions', path: '/permissions', icon: IconShield },
]

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const { channel } = useParams<{ channel: string }>()
  const pathname = usePathname()

  return (
    <Grid>
      <Grid.Col span={{ base: 12, sm: 3 }}>
        <Card withBorder padding="xs" radius="md">
          <Stack gap={2}>
            <Title order={5} p="xs">Admin</Title>
            {sections.map(section => (
              <NavLink
                key={section.path}
                component={Link}
                href={`/${channel}/admin${section.path}`}
                label={section.label}
                leftSection={<section.icon size={16} />}
                active={pathname === `/${channel}/admin${section.path}`}
              />
            ))}
          </Stack>
        </Card>
      </Grid.Col>

      <Grid.Col span={{ base: 12, sm: 9 }}>
        {children}
      </Grid.Col>
    </Grid>
  )
}
