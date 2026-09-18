'use client'

import QueryState from '@/components/channel/QueryState'
import { doDelete, doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { SubathonRate } from '@/interfaces/api/admin/AdminTypes'
import {
  Alert,
  Button,
  Card,
  Group,
  Modal,
  NumberInput,
  Stack,
  Table,
  Text,
  Title
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconCheck, IconPlus, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'next/navigation'
import { useState } from 'react'

export default function AdminSubathonPage() {
  const { channel } = useParams<{ channel: string }>()
  const queryClient = useQueryClient()
  const [opened, { open, close }] = useDisclosure(false)

  const [editing, setEditing] = useState<Partial<SubathonRate>>({})

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.SubathonRates, channel],
    queryFn: async () => await doQueryGet<SubathonRate[]>(`/api/Channels/${channel}/Admin/SubathonRates`)
  })

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: [QueryKeys.SubathonRates, channel] })
  }

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/SubathonRates`, { body: editing })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save that band')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Rate band saved', 4000, <IconCheck />)
      invalidate()
      close()
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  const remove = useMutation({
    mutationFn: async (id: number) => {
      const res = await doDelete(`/api/Channels/${channel}/Admin/SubathonRates/${id}`)

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not delete that band')
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
          <Title order={3}>Subathon rates</Title>
          <Text size="sm" c="dimmed">
            How much time bits and subs are worth once the subathon reaches each total.
          </Text>
        </Stack>
        <Button
          leftSection={<IconPlus size={16} />}
          onClick={() => {
            setEditing({ fromHours: 0, millisecondsPerBit: 0, tier1SubMinutes: 0, tier2SubMinutes: 0, tier3SubMinutes: 0 })
            open()
          }}
        >
          Add band
        </Button>
      </Group>

      <Alert variant="light" p="xs">
        <Text size="xs">
          The band used is the one with the highest starting hours at or below the subathon&apos;s
          current total, so rates taper as it grows. The band starting at 0 hours cannot be removed.
        </Text>
      </Alert>

      <QueryState
        isLoading={isLoading}
        isError={isError}
        isEmpty={data?.length === 0}
        emptyMessage="No rate bands configured. A subathon would earn no time at all."
        errorMessage="You do not have permission to change the subathon."
      >
        <Card withBorder padding={0} radius="md">
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>From</Table.Th>
                <Table.Th ta="right">ms per bit</Table.Th>
                <Table.Th ta="right">Tier 1</Table.Th>
                <Table.Th ta="right">Tier 2</Table.Th>
                <Table.Th ta="right">Tier 3</Table.Th>
                <Table.Th w={160} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {data?.map(rate => (
                <Table.Tr key={rate.id}>
                  <Table.Td><Text fw={600}>{rate.fromHours}h</Text></Table.Td>
                  <Table.Td ta="right">{rate.millisecondsPerBit}</Table.Td>
                  <Table.Td ta="right">{rate.tier1SubMinutes} min</Table.Td>
                  <Table.Td ta="right">{rate.tier2SubMinutes} min</Table.Td>
                  <Table.Td ta="right">{rate.tier3SubMinutes} min</Table.Td>
                  <Table.Td>
                    <Group gap="xs" justify="flex-end">
                      <Button size="xs" variant="subtle" onClick={() => { setEditing(rate); open() }}>Edit</Button>
                      <Button
                        size="xs"
                        variant="subtle"
                        color="red"
                        disabled={rate.fromHours === 0}
                        onClick={() => remove.mutate(rate.id)}
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

      <Modal opened={opened} onClose={close} title={editing.id !== undefined ? 'Edit band' : 'Add band'}>
        <Stack gap="md">
          <NumberInput
            label="From hours"
            description="This band applies once the subathon reaches this many hours"
            value={editing.fromHours ?? 0}
            onChange={value => setEditing({ ...editing, fromHours: Number(value) })}
            min={0}
          />

          <NumberInput
            label="Milliseconds per bit"
            value={editing.millisecondsPerBit ?? 0}
            onChange={value => setEditing({ ...editing, millisecondsPerBit: Number(value) })}
            min={0}
          />

          <Group grow>
            <NumberInput
              label="Tier 1 minutes"
              value={editing.tier1SubMinutes ?? 0}
              onChange={value => setEditing({ ...editing, tier1SubMinutes: Number(value) })}
              min={0}
            />
            <NumberInput
              label="Tier 2 minutes"
              value={editing.tier2SubMinutes ?? 0}
              onChange={value => setEditing({ ...editing, tier2SubMinutes: Number(value) })}
              min={0}
            />
            <NumberInput
              label="Tier 3 minutes"
              value={editing.tier3SubMinutes ?? 0}
              onChange={value => setEditing({ ...editing, tier3SubMinutes: Number(value) })}
              min={0}
            />
          </Group>

          <Group justify="flex-end">
            <Button variant="default" onClick={close}>Cancel</Button>
            <Button onClick={() => save.mutate()} loading={save.isPending}>Save</Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
