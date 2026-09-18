'use client'

import QueryState from '@/components/channel/QueryState'
import { doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { ChannelConfig } from '@/interfaces/api/admin/AdminTypes'
import { Button, Card, Group, NumberInput, Stack, TextInput, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'next/navigation'
import { useEffect, useState } from 'react'

export default function AdminSettingsPage() {
  const { channel } = useParams<{ channel: string }>()
  const queryClient = useQueryClient()

  const [currencyName, setCurrencyName] = useState('')
  const [pointCap, setPointCap] = useState<number | string>(0)

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.ChannelConfig, channel],
    queryFn: async () => await doQueryGet<ChannelConfig>(`/api/Channels/${channel}/Admin/Config`)
  })

  useEffect(() => {
    if (data !== undefined) {
      setCurrencyName(data.channelCurrencyName)
      setPointCap(data.currencyPointCap)
    }
  }, [data])

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Config`, {
        body: { channelCurrencyName: currencyName, currencyPointCap: Number(pointCap) }
      })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Channel settings updated', 4000, <IconCheck />)
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.ChannelConfig, channel] })
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Title order={3}>Settings</Title>

      <QueryState isLoading={isLoading} isError={isError} errorMessage="You do not have permission to change these settings.">
        <Card withBorder padding="lg" radius="md">
          <Stack gap="md">
            <TextInput
              label="Currency name"
              description="What your channel's points are called"
              value={currencyName}
              onChange={event => setCurrencyName(event.currentTarget.value)}
            />

            <NumberInput
              label="Point cap"
              description="The most a viewer can hold, and what a prestige costs"
              value={pointCap}
              onChange={setPointCap}
              min={1}
              thousandSeparator=","
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
