'use client'

import QueryState from '@/components/channel/QueryState'
import { doQueryGet, doPut } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { channelPermissions, PermissionHolder } from '@/interfaces/api/admin/Permissions'
import {
  Alert,
  Button,
  Card,
  Checkbox,
  Group,
  Modal,
  Stack,
  Table,
  Text,
  TextInput,
  Title
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'next/navigation'
import { useState } from 'react'

export default function PermissionsPage() {
  const { channel } = useParams<{ channel: string }>()
  const queryClient = useQueryClient()
  const [opened, { open, close }] = useDisclosure(false)

  const [username, setUsername] = useState('')
  const [selected, setSelected] = useState<string[]>([])

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelPermissions, channel],
    queryFn: async () => await doQueryGet<PermissionHolder[]>(`/api/Channels/${channel}/Permissions`)
  })

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Permissions`, {
        body: { twitchUsername: username, permissions: selected }
      })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save those permissions')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', `Permissions updated for ${username}`, 4000, <IconCheck />)
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.ChannelPermissions, channel] })
      close()
      setUsername('')
      setSelected([])
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  const editHolder = (holder: PermissionHolder) => {
    setUsername(holder.twitchUsername)
    setSelected(holder.permissions)
    open()
  }

  return (
    <Stack gap="lg">
      <Group justify="space-between">
        <Stack gap={2}>
          <Title order={3}>Permissions</Title>
          <Text size="sm" c="dimmed">Let specific mods change specific things. Only you can edit this.</Text>
        </Stack>
        <Button leftSection={<IconPlus size={16} />} onClick={() => { setUsername(''); setSelected([]); open() }}>
          Add
        </Button>
      </Group>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="Nobody else has permissions in this channel yet."
        errorMessage="Only the broadcaster can manage permissions."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>User</Table.Th>
                <Table.Th>Permissions</Table.Th>
                <Table.Th w={100} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(holder => (
                <Table.Tr key={holder.twitchUserId}>
                  <Table.Td>{holder.twitchUsername}</Table.Td>
                  <Table.Td>
                    <Text size="sm" c="dimmed">
                      {holder.permissions
                        .filter(p => p !== 'ViewAdmin')
                        .map(p => channelPermissions.find(x => x.value === p)?.label ?? p)
                        .join(', ')}
                    </Text>
                  </Table.Td>
                  <Table.Td>
                    <Button size="xs" variant="subtle" onClick={() => editHolder(holder)}>Edit</Button>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Card>
      </QueryState>

      <Modal opened={opened} onClose={close} title="Set permissions" size="md">
        <Stack gap="md">
          <TextInput
            label="Twitch username"
            placeholder="theirusername"
            value={username}
            onChange={event => setUsername(event.currentTarget.value)}
          />

          <Alert variant="light" p="xs">
            <Text size="xs">
              Granting anything gives access to the admin area. Remove every permission to revoke it.
            </Text>
          </Alert>

          <Stack gap="xs">
            {channelPermissions
              .filter(permission => permission.value !== 'ViewAdmin')
              .map(permission => (
                <Checkbox
                  key={permission.value}
                  label={permission.label}
                  description={permission.description}
                  checked={selected.includes(permission.value)}
                  onChange={event => {
                    setSelected(current => event.currentTarget.checked
                      ? [...current, permission.value]
                      : current.filter(x => x !== permission.value))
                  }}
                />
              ))}
          </Stack>

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>Cancel</Button>
            <Button
              onClick={() => save.mutate()}
              loading={save.isPending}
              disabled={username.trim() === ''}
            >
              Save
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
