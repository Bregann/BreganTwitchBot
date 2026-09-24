'use client'

import QueryState from '@/components/channel/QueryState'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { StreamHistoryItem } from '@/interfaces/api/channel/ChannelTypes'
import { AreaChart } from '@mantine/charts'
import { Card, ScrollArea, Stack, Table, Text, Title } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import dayjs from 'dayjs'

export default function ChannelStatsComponent({ channel }: { channel: string }) {

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelStats, channel],
    queryFn: async () => await doQueryGet<StreamHistoryItem[]>(`/api/Channels/${channel}/streams?take=30`)
  })

  // the api returns newest first, charts read better oldest first
  const chartData = [...(data ?? [])].reverse().map(stream => ({
    date: dayjs(stream.streamStarted).format('DD MMM'),
    Average: Math.round(stream.avgViewCount),
    Peak: stream.peakViewerCount
  }))

  return (
    <QueryState
      isLoading={isLoading}
      isError={isError}
      isEmpty={data?.length === 0}
      emptyMessage="No streams have been recorded for this channel yet."
    >
      <Stack gap="xl">
        <Card withBorder padding="md" radius="md">
          <Stack gap="md">
            <Title order={4}>Viewers over the last {chartData.length} streams</Title>
            <AreaChart
              h={260}
              data={chartData}
              dataKey="date"
              series={[
                { name: 'Average', color: 'twitch.5' },
                { name: 'Peak', color: 'blue.5' }
              ]}
              curveType="monotone"
              withLegend
            />
          </Stack>
        </Card>

        <Card withBorder padding={0} radius="md">
          <ScrollArea>
            <Table highlightOnHover miw={760}>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>Stream</Table.Th>
                  <Table.Th>Date</Table.Th>
                  <Table.Th>Uptime</Table.Th>
                  <Table.Th ta="right">Avg</Table.Th>
                  <Table.Th ta="right">Peak</Table.Th>
                  <Table.Th ta="right">Chatters</Table.Th>
                  <Table.Th ta="right">Messages</Table.Th>
                  <Table.Th ta="right">Follows</Table.Th>
                  <Table.Th ta="right">Subs</Table.Th>
                  <Table.Th ta="right">Bits</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {data?.map(stream => (
                  <Table.Tr key={stream.streamId}>
                    <Table.Td><Text fw={600}>#{stream.streamId}</Text></Table.Td>
                    <Table.Td>{dayjs(stream.streamStarted).format('DD MMM YYYY')}</Table.Td>
                    <Table.Td>{stream.uptime}</Table.Td>
                    <Table.Td ta="right">{Math.round(stream.avgViewCount).toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.peakViewerCount.toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.uniquePeople.toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.messagesReceived.toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.newFollowers.toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.newSubscribers.toLocaleString()}</Table.Td>
                    <Table.Td ta="right">{stream.bitsDonated.toLocaleString()}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </ScrollArea>
        </Card>
      </Stack>
    </QueryState>
  )
}
