'use client'

import { Card, SimpleGrid, Stack, Text, Title } from '@mantine/core'
import Link from 'next/link'

const sections = [
  { label: 'Settings', path: '/settings', description: 'Currency name and point cap' },
  { label: 'Commands', path: '/commands', description: 'Custom chat commands' },
  { label: 'Ranks', path: '/ranks', description: 'Watchtime ranks and their rewards' },
  { label: 'Blacklist', path: '/blacklist', description: 'Words that warn, time out or ban' },
  { label: 'Discord', path: '/discord', description: 'Guild, channels and roles' },
  { label: 'Permissions', path: '/permissions', description: 'Who can change what' },
]

export default function AdminHomeComponent({ channel }: { channel: string }) {

  return (
    <Stack gap="lg">
      <Title order={3}>Admin</Title>

      <SimpleGrid cols={{ base: 1, sm: 2 }}>
        {sections.map(section => (
          <Card
            key={section.path}
            component={Link}
            href={`/${channel}/admin${section.path}`}
            withBorder
            padding="lg"
            radius="md"
          >
            <Stack gap={4}>
              <Text fw={600}>{section.label}</Text>
              <Text size="sm" c="dimmed">{section.description}</Text>
            </Stack>
          </Card>
        ))}
      </SimpleGrid>
    </Stack>
  )
}
