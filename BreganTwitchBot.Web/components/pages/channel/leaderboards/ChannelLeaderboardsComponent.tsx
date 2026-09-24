'use client'

import QueryState from '@/components/channel/QueryState'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { hourBasedLeaderboards, Leaderboard, leaderboardTypes } from '@/interfaces/api/channel/ChannelTypes'
import { Card, Group, SegmentedControl, ScrollArea, Stack, Table, Text } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

export default function ChannelLeaderboardsComponent({ channel }: { channel: string }) {
  const [type, setType] = useState<string>('Points')

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelLeaderboard, channel, type],
    queryFn: async () => await doQueryGet<Leaderboard>(`/api/Leaderboards/${channel}/${type}?take=100`)
  })

  const formatValue = (value: number): string => {
    if (hourBasedLeaderboards.includes(type)) {
      return `${(value / 60).toLocaleString(undefined, { maximumFractionDigits: 1 })} hours`
    }

    return value.toLocaleString()
  }

  return (
    <Stack gap="lg">
      <ScrollArea>
        <SegmentedControl
          value={type}
          onChange={setType}
          data={leaderboardTypes.map(x => ({ value: x.value, label: x.label }))}
        />
      </ScrollArea>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.positions.length === 0}
        emptyMessage="Nobody is on this leaderboard yet."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th w={80}>#</Table.Th>
                <Table.Th>User</Table.Th>
                <Table.Th ta="right">Value</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.positions.map(position => (
                <Table.Tr key={`${position.position}-${position.username}`}>
                  <Table.Td>
                    <Group gap={4}>
                      <Text fw={position.position <= 3 ? 700 : 400}>{position.position}</Text>
                    </Group>
                  </Table.Td>
                  <Table.Td>{position.username}</Table.Td>
                  <Table.Td ta="right">{formatValue(position.value)}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>
    </Stack>
  )
}
