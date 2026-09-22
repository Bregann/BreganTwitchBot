'use client'

import QueryState from '@/components/channel/QueryState'
import { doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { GiveawayConfig } from '@/interfaces/api/admin/AdminTypes'
import { Button, Card, Group, NumberInput, Stack, Switch, Text, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

export default function AdminGiveawaysComponent({ channel }: { channel: string }) {
  const queryClient = useQueryClient()

  const [config, setConfig] = useState<GiveawayConfig | null>(null)

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.GiveawayConfig, channel],
    queryFn: async () => await doQueryGet<GiveawayConfig>(`/api/Channels/${channel}/Admin/Giveaways`)
  })

  useEffect(() => {
    if (data !== undefined) {
      setConfig(data)
    }
  }, [data])

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Giveaways`, { body: config })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save the giveaway settings')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Giveaway settings updated', 4000, <IconCheck />)
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.GiveawayConfig, channel] })
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Stack gap={2}>
        <Title order={3}>Giveaways</Title>
        <Text size="sm" c="dimmed">
          How entries are weighted. Everyone who meets a giveaway&apos;s requirements gets at least one.
        </Text>
      </Stack>

      <QueryState isLoading={isLoading} isError={isError} errorMessage="You do not have permission to change giveaways.">
        <Card withBorder padding="lg" radius="md">
          <Stack gap="md">
            <NumberInput
              label="Minutes per entry"
              description="Watchtime needed to earn one extra entry"
              value={config?.minutesPerEntry ?? 0}
              onChange={value => setConfig(current => current === null ? current : { ...current, minutesPerEntry: Number(value) })}
              min={1}
              thousandSeparator=","
            />

            <NumberInput
              label="XP per entry"
              description="Discord xp needed to earn one extra entry"
              value={config?.xpPerEntry ?? 0}
              onChange={value => setConfig(current => current === null ? current : { ...current, xpPerEntry: Number(value) })}
              min={1}
              thousandSeparator=","
            />

            <NumberInput
              label="Maximum xp entries"
              description="The most entries Discord xp alone can earn"
              value={config?.maxXpEntries ?? 0}
              onChange={value => setConfig(current => current === null ? current : { ...current, maxXpEntries: Number(value) })}
              min={0}
            />

            <Switch
              label="Ranks grant entries"
              description="Each stream rank earned gives an extra entry"
              checked={config?.ranksGrantEntries ?? true}
              onChange={event => setConfig(current => current === null ? current : { ...current, ranksGrantEntries: event.currentTarget.checked })}
            />

            <Group justify="flex-end">
              <Button onClick={() => save.mutate()} loading={save.isPending}>Save</Button>
            </Group>
          </Stack>
        </Card>
      </QueryState>
    </Stack>
  )
}
