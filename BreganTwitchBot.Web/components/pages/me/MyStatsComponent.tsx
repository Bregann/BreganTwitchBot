'use client'

import QueryState from '@/components/channel/QueryState'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { useAuth } from '@/context/authContext'
import { MyChannelStats, MyStats } from '@/interfaces/api/me/MyStats'
import {
  Alert,
  Anchor,
  Badge,
  Card,
  Grid,
  Group,
  Progress,
  Stack,
  Text,
  Title
} from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

export default function MyStatsComponent() {
  const { isAuthenticated, isLoading: authLoading } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace('/login')
    }
  }, [authLoading, isAuthenticated, router])

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.MyStats],
    queryFn: async () => await doQueryGet<MyStats>('/api/Me/Stats'),
    enabled: isAuthenticated
  })

  return (
    <Stack gap="lg">
      <Title order={2}>Your stats</Title>

      <QueryState isLoading={authLoading || isLoading} isError={isError}>
        {data?.channels.length === 0 && (
          <Alert variant="light">
            The bot hasn&apos;t seen you in any channels yet. Chat in a channel running the bot
            and your stats will show up here.
          </Alert>
        )}

        <Grid>
          {data?.channels.map(channel => (
            <Grid.Col key={channel.broadcasterChannelName} span={{ base: 12, md: 6 }}>
              <ChannelStatsCard channel={channel} />
            </Grid.Col>
          ))}
        </Grid>
      </QueryState>
    </Stack>
  )
}

function ChannelStatsCard({ channel }: { channel: MyChannelStats }) {
  const hours = (channel.minutesInStream / 60).toLocaleString(undefined, { maximumFractionDigits: 1 })

  // how far through the current rank they are, if there's a next one to reach
  const rankProgress = channel.minutesUntilNextRank !== null && channel.minutesUntilNextRank > 0
    ? (channel.minutesInStream / (channel.minutesInStream + channel.minutesUntilNextRank)) * 100
    : null

  return (
    <Card withBorder padding="lg" radius="md" h="100%">
      <Stack gap="md">
        <Group justify="space-between">
          <Anchor component={Link} href={`/${channel.broadcasterChannelName}`} fw={600} tt="capitalize">
            {channel.broadcasterChannelName}
          </Anchor>
          {channel.currentRank !== null && (
            <Badge variant="light">{channel.currentRank}</Badge>
          )}
        </Group>

        <Grid gap="xs">
          <Stat label={channel.pointsName} value={channel.points.toLocaleString()} />
          <Stat label="Watchtime" value={`${hours} hours`} />
          <Stat label="Messages" value={channel.totalMessages.toLocaleString()} />
          <Stat label="Daily streak" value={channel.currentDailyStreak.toLocaleString()} />
        </Grid>

        {rankProgress !== null && (
          <Stack gap={4}>
            <Group justify="space-between">
              <Text size="xs" c="dimmed">Next rank: {channel.nextRank}</Text>
              <Text size="xs" c="dimmed">
                {Math.round((channel.minutesUntilNextRank ?? 0) / 60)}h to go
              </Text>
            </Group>
            <Progress value={rankProgress} size="sm" radius="xl" />
          </Stack>
        )}

        {channel.discordLevel !== null && (
          <Text size="xs" c="dimmed">
            Discord level {channel.discordLevel} ({(channel.discordXp ?? 0).toLocaleString()} xp)
          </Text>
        )}
      </Stack>
    </Card>
  )
}

function Stat({ label, value }: { label: string, value: string }) {
  return (
    <Grid.Col span={6}>
      <Text size="xs" c="dimmed" tt="uppercase" fw={600}>{label}</Text>
      <Text fw={600}>{value}</Text>
    </Grid.Col>
  )
}
