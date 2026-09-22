'use client'

import QueryState from '@/components/channel/QueryState'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { CustomCommand } from '@/interfaces/api/channel/ChannelTypes'
import { Card, Table, Text } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'

export default function ChannelCommandsComponent({ channel }: { channel: string }) {

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelCommands, channel],
    queryFn: async () => await doQueryGet<CustomCommand[]>(`/api/Commands/${channel}`)
  })

  return (
    <QueryState
      isLoading={isLoading}
      isError={isError}
      isEmpty={data?.length === 0}
      emptyMessage="This channel has no custom commands yet."
    >
      <Card withBorder padding={0} radius="md">
        <Table highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th w={200}>Command</Table.Th>
              <Table.Th>Response</Table.Th>
              <Table.Th w={120} ta="right">Times used</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {data?.map(command => (
              <Table.Tr key={command.commandName}>
                <Table.Td><Text ff="monospace">{command.commandName}</Text></Table.Td>
                <Table.Td>{command.commandText}</Table.Td>
                <Table.Td ta="right">{command.timesUsed.toLocaleString()}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Card>
    </QueryState>
  )
}
