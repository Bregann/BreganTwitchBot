'use client'

import QueryState from '@/components/channel/QueryState'
import { doDelete, doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { CustomCommand } from '@/interfaces/api/channel/ChannelTypes'
import { Button, Card, Group, Modal, Stack, Table, Text, Textarea, TextInput, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'next/navigation'
import { useState } from 'react'

export default function AdminCommandsPage() {
  const { channel } = useParams<{ channel: string }>()
  const queryClient = useQueryClient()
  const [opened, { open, close }] = useDisclosure(false)

  const [commandName, setCommandName] = useState('')
  const [commandText, setCommandText] = useState('')
  const [isEditing, setIsEditing] = useState(false)

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelCommands, channel],
    queryFn: async () => await doQueryGet<CustomCommand[]>(`/api/Commands/${channel}`)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.ChannelCommands, channel] })
  }

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Commands`, {
        body: { commandName, commandText }
      })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save that command')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', `${commandName} saved`, 4000, <IconCheck />)
      invalidate()
      close()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  const remove = useMutation({
    mutationFn: async (name: string) => {
      const res = await doDelete(`/api/Channels/${channel}/Admin/Commands/${encodeURIComponent(name)}`)

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not delete that command')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Deleted', 'Command removed', 4000, <IconCheck />)
      invalidate()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not delete', error.message, 6000, <IconX />)
    }
  })

  const startAdd = () => {
    setCommandName('')
    setCommandText('')
    setIsEditing(false)
    open()
  }

  const startEdit = (command: CustomCommand) => {
    setCommandName(command.commandName)
    setCommandText(command.commandText)
    setIsEditing(true)
    open()
  }

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Title order={3}>Custom commands</Title>
        <Button leftSection={<IconPlus size={16} />} onClick={startAdd}>Add</Button>
      </Group>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="No custom commands yet."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th w={180}>Command</Table.Th>
                <Table.Th>Response</Table.Th>
                <Table.Th w={160} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(command => (
                <Table.Tr key={command.commandName}>
                  <Table.Td><Text ff="monospace">{command.commandName}</Text></Table.Td>
                  <Table.Td>{command.commandText}</Table.Td>
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <Button size="xs" variant="subtle" onClick={() => startEdit(command)}>Edit</Button>
                      <Button
                        size="xs"
                        variant="subtle"
                        color="red"
                        loading={remove.isPending}
                        onClick={() => remove.mutate(command.commandName)}
                      >
                        Delete
                      </Button>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>

      <Modal opened={opened} onClose={close} title={isEditing ? 'Edit command' : 'Add command'}>
        <Stack gap="md">
          <TextInput
            label="Command"
            description="The ! is added automatically"
            placeholder="discord"
            value={commandName}
            onChange={event => setCommandName(event.currentTarget.value)}
            disabled={isEditing}
          />

          <Textarea
            label="Response"
            placeholder="What the bot replies with"
            value={commandText}
            onChange={event => setCommandText(event.currentTarget.value)}
            autosize
            minRows={2}
          />

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>Cancel</Button>
            <Button
              onClick={() => save.mutate()}
              loading={save.isPending}
              disabled={commandName.trim() === '' || commandText.trim() === ''}
            >
              Save
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
