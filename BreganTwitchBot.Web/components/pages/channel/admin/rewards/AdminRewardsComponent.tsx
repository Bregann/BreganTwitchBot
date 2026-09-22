'use client'

import QueryState from '@/components/channel/QueryState'
import { doDelete, doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { ChannelPointReward } from '@/interfaces/api/admin/AdminTypes'
import {
  Badge,
  Button,
  Card,
  Group,
  Modal,
  Stack,
  Switch,
  Table,
  Text,
  Textarea,
  TextInput,
  Title
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

export default function AdminRewardsComponent({ channel }: { channel: string }) {
  const queryClient = useQueryClient()
  const [opened, { open, close }] = useDisclosure(false)

  const [editing, setEditing] = useState<Partial<ChannelPointReward>>({})

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelRewards, channel],
    queryFn: async () => await doQueryGet<ChannelPointReward[]>(`/api/Channels/${channel}/Admin/Rewards`)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.ChannelRewards, channel] })
  }

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Rewards`, { body: editing })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save that reward')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Reward saved', 4000, <IconCheck />)
      invalidate()
      close()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await doDelete(`/api/Channels/${channel}/Admin/Rewards/${id}`)

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not delete that reward')
      }
    },
    onSuccess: invalidate,
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not delete', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Stack gap={2}>
          <Title order={3}>Channel point rewards</Title>
          <Text size="sm" c="dimmed">
            When a viewer redeems a reward with a matching title, the bot posts your message.
          </Text>
        </Stack>
        <Button
          leftSection={<IconPlus size={16} />}
          onClick={() => { setEditing({ rewardTitle: '', responseMessage: '', enabled: true }); open() }}
        >
          Add
        </Button>
      </Group>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="No rewards configured yet."
        errorMessage="You do not have permission to change rewards."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Reward title</Table.Th>
                <Table.Th>Message</Table.Th>
                <Table.Th w={100} ta="right">Redeemed</Table.Th>
                <Table.Th w={160} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(reward => (
                <Table.Tr key={reward.id}>
                  <Table.Td>
                    <Group gap="xs">
                      <Text fw={600}>{reward.rewardTitle}</Text>
                      {!reward.enabled && <Badge size="xs" color="gray">Off</Badge>}
                    </Group>
                  </Table.Td>
                  <Table.Td><Text size="sm" c="dimmed">{reward.responseMessage}</Text></Table.Td>
                  <Table.Td ta="right">{reward.timesRedeemed.toLocaleString()}</Table.Td>
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <Button size="xs" variant="subtle" onClick={() => { setEditing(reward); open() }}>Edit</Button>
                      <Button size="xs" variant="subtle" color="red" onClick={() => remove.mutate(reward.id)}>Delete</Button>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>

      <Modal opened={opened} onClose={close} title={editing.id !== undefined ? 'Edit reward' : 'Add reward'}>
        <Stack gap="md">
          <TextInput
            label="Reward title"
            description="Must match the reward name on Twitch exactly"
            value={editing.rewardTitle ?? ''}
            onChange={event => setEditing({ ...editing, rewardTitle: event.currentTarget.value })}
          />

          <Textarea
            label="Message"
            description="Use {user} for whoever redeemed it"
            value={editing.responseMessage ?? ''}
            onChange={event => setEditing({ ...editing, responseMessage: event.currentTarget.value })}
            autosize
            minRows={2}
          />

          <Switch
            label="Enabled"
            checked={editing.enabled ?? true}
            onChange={event => setEditing({ ...editing, enabled: event.currentTarget.checked })}
          />

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>Cancel</Button>
            <Button
              onClick={() => save.mutate()}
              loading={save.isPending}
              disabled={(editing.rewardTitle ?? '').trim() === '' || (editing.responseMessage ?? '').trim() === ''}
            >
              Save
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
