'use client'

import QueryState from '@/components/channel/QueryState'
import StatCard from '@/components/channel/StatCard'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { ChannelSummary } from '@/interfaces/api/channel/ChannelTypes'
import { Badge, Grid, Group, Stack } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'

export default function ChannelOverviewComponent({ channel }: { channel: string }) {

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelSummary, channel],
    queryFn: async () => await doQueryGet<ChannelSummary>(`/api/Channels/${channel}`)
  })

  return (
    <QueryState isLoading={isLoading} isError={isError} errorMessage="That channel could not be found.">
      <Stack gap="lg">
        <Group>
          <Badge color={data?.isLive === true ? 'red' : 'gray'} variant="filled" size="lg">
            {data?.isLive === true ? 'Live' : 'Offline'}
          </Badge>
          {data?.subathonActive === true && (
            <Badge color="grape" variant="light" size="lg">Subathon running</Badge>
          )}
        </Group>

        <Grid>
          <Grid.Col span={{ base: 6, sm: 3 }}>
            <StatCard label="Tracked viewers" value={data?.trackedViewers ?? 0} />
          </Grid.Col>
          <Grid.Col span={{ base: 6, sm: 3 }}>
            <StatCard label="Streams" value={data?.totalStreams ?? 0} />
          </Grid.Col>
          <Grid.Col span={{ base: 6, sm: 3 }}>
            <StatCard label="Currency" value={data?.pointsName ?? '-'} />
          </Grid.Col>
          <Grid.Col span={{ base: 6, sm: 3 }}>
            <StatCard label="Subathon" value={data?.subathonActive === true ? 'Running' : 'Not running'} />
          </Grid.Col>
        </Grid>
      </Stack>
    </QueryState>
  )
}
