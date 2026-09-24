'use client'

import QueryState from '@/components/channel/QueryState'
import { doDelete, doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { ChannelRank } from '@/interfaces/api/admin/AdminTypes'
import { Button, Card, Group, Modal, NumberInput, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

export default function AdminRanksComponent({ channel }: { channel: string }) {
  const queryClient = useQueryClient()
  const [opened, { open, close }] = useDisclosure(false)

  const [editing, setEditing] = useState<Partial<ChannelRank>>({})

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelRanks, channel],
    queryFn: async () => await doQueryGet<ChannelRank[]>(`/api/Channels/${channel}/Admin/Ranks`)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.ChannelRanks, channel] })
  }

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Ranks`, { body: editing })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save that rank')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Rank saved', 4000, <IconCheck />)
      invalidate()
      close()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await doDelete(`/api/Channels/${channel}/Admin/Ranks/${id}`)

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not delete that rank')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Deleted', 'Rank removed', 4000, <IconCheck />)
      invalidate()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not delete', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={3}>Ranks</Title>
        <Button
          leftSection={<IconPlus size={16} />}
          onClick={() => { setEditing({ rankName: '', rankMinutesRequired: 0, bonusRankPointsEarned: 0 }); open() }}
        >
          Add
        </Button>
      </Group>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="No ranks configured yet."
        errorMessage="You do not have permission to change ranks."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Rank</Table.Th>
                <Table.Th ta="right">Hours needed</Table.Th>
                <Table.Th ta="right">Bonus points</Table.Th>
                <Table.Th w={160} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(rank => (
                <Table.Tr key={rank.id}>
                  <Table.Td><Text fw={600}>{rank.rankName}</Text></Table.Td>
                  <Table.Td ta="right">{Math.round(rank.rankMinutesRequired / 60).toLocaleString()}</Table.Td>
                  <Table.Td ta="right">{rank.bonusRankPointsEarned.toLocaleString()}</Table.Td>
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <Button size="xs" variant="subtle" onClick={() => { setEditing(rank); open() }}>Edit</Button>
                      <Button size="xs" variant="subtle" color="red" onClick={() => remove.mutate(rank.id)}>Delete</Button>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>

      <Modal opened={opened} onClose={close} title={editing.id !== undefined ? 'Edit rank' : 'Add rank'}>
        <Stack gap="md">
          <TextInput
            label="Rank name"
            value={editing.rankName ?? ''}
            onChange={event => setEditing({ ...editing, rankName: event.currentTarget.value })}
          />

          <NumberInput
            label="Minutes required"
            description="How much watchtime earns this rank"
            value={editing.rankMinutesRequired ?? 0}
            onChange={value => setEditing({ ...editing, rankMinutesRequired: Number(value) })}
            min={0}
            thousandSeparator=","
          />

          <NumberInput
            label="Bonus points"
            description="Awarded once when the rank is reached"
            value={editing.bonusRankPointsEarned ?? 0}
            onChange={value => setEditing({ ...editing, bonusRankPointsEarned: Number(value) })}
            min={0}
            thousandSeparator=","
          />

          <NumberInput
            label="Discord role id"
            description="Optional. Applied when the rank is earned"
            value={editing.discordRoleId ?? ''}
            onChange={value => setEditing({ ...editing, discordRoleId: value === '' ? null : Number(value) })}
            hideControls
          />

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>Cancel</Button>
            <Button
              onClick={() => save.mutate()}
              loading={save.isPending}
              disabled={(editing.rankName ?? '').trim() === ''}
            >
              Save
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
