'use client'

import { Group, Tabs, Title } from '@mantine/core'
import { useRouter, useSelectedLayoutSegment } from 'next/navigation'

const tabs = [
  { value: 'overview', label: 'Overview', path: '' },
  { value: 'leaderboards', label: 'Leaderboards', path: '/leaderboards' },
  { value: 'subathon', label: 'Subathon', path: '/subathon' },
  { value: 'stats', label: 'Stats', path: '/stats' },
  { value: 'commands', label: 'Commands', path: '/commands' },
]

export default function ChannelNav({ channel }: { channel: string }) {
  const router = useRouter()
  const segment = useSelectedLayoutSegment()

  // the overview page has no segment of its own
  const active = segment ?? 'overview'

  return (
    <>
      <Group mb="md">
        <Title order={2} tt="capitalize">{channel}</Title>
      </Group>

      <Tabs
        value={active}
        onChange={value => {
          const tab = tabs.find(x => x.value === value)

          if (tab !== undefined) {
            router.push(`/${channel}${tab.path}`)
          }
        }}
        mb="lg"
      >
        <Tabs.List>
          {tabs.map(tab => (
            <Tabs.Tab key={tab.value} value={tab.value}>{tab.label}</Tabs.Tab>
          ))}
        </Tabs.List>
      </Tabs>
    </>
  )
}
