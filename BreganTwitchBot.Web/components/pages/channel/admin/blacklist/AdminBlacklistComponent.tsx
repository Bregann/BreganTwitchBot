'use client'

import QueryState from '@/components/channel/QueryState'
import { doDelete, doPost, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { BlacklistWord, WordType, wordTypes } from '@/interfaces/api/admin/AdminTypes'
import { Badge, Button, Card, Group, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

const badgeColours: Record<WordType, string> = {
  StrikeWord: 'yellow',
  TempBanWord: 'orange',
  PermBanWord: 'red'
}

export default function AdminBlacklistComponent({ channel }: { channel: string }) {
  const queryClient = useQueryClient()

  const [word, setWord] = useState('')
  const [wordType, setWordType] = useState<WordType>('StrikeWord')

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.WordBlacklist, channel],
    queryFn: async () => await doQueryGet<BlacklistWord[]>(`/api/Channels/${channel}/Admin/Blacklist`)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.WordBlacklist, channel] })
  }

  const add = useMutation({
    mutationFn: async () => {
      const res = await doPost(`/api/Channels/${channel}/Admin/Blacklist`, { body: { word, wordType } })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not add that word')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Added', 'Word added to the blacklist', 4000, <IconCheck />)
      setWord('')
      invalidate()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not add', error.message, 6000, <IconX />)
    }
  })

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await doDelete(`/api/Channels/${channel}/Admin/Blacklist/${id}`)

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not remove that word')
      }
    },
    onSuccess: invalidate,
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not remove', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Title order={3}>Word blacklist</Title>

      <Card withBorder padding="md" radius="md">
        <Group align="flex-end">
          <TextInput
            label="Word"
            placeholder="A word to act on"
            value={word}
            onChange={event => setWord(event.currentTarget.value)}
            style={{ flex: 1 }}
          />

          <Select
            label="Action"
            value={wordType}
            onChange={value => setWordType((value ?? 'StrikeWord') as WordType)}
            data={wordTypes.map(x => ({ value: x.value, label: x.label }))}
            w={140}
          />

          <Button onClick={() => add.mutate()} loading={add.isPending} disabled={word.trim() === ''}>
            Add
          </Button>
        </Group>
      </Card>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="Nothing is blacklisted in this channel."
        errorMessage="You do not have permission to change the blacklist."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Word</Table.Th>
                <Table.Th w={120}>Action</Table.Th>
                <Table.Th w={100} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(item => (
                <Table.Tr key={item.id}>
                  <Table.Td><Text ff="monospace">{item.word}</Text></Table.Td>
                  <Table.Td>
                    <Badge color={badgeColours[item.wordType]} variant="light">
                      {wordTypes.find(x => x.value === item.wordType)?.label ?? item.wordType}
                    </Badge>
                  </Table.Td>
                  <Table.Td>
                    <Button size="xs" variant="subtle" color="red" onClick={() => remove.mutate(item.id)}>
                      Remove
                    </Button>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>
    </Stack>
  )
}
