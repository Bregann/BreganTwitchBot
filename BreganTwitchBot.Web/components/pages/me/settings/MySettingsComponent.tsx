'use client'

import QueryState from '@/components/channel/QueryState'
import { useAuth } from '@/context/authContext'
import { doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { MyChannelSetting, MySettings } from '@/interfaces/api/me/MyStats'
import { Card, Stack, Switch, Text, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

export default function MySettingsComponent() {
  const { isAuthenticated, isLoading: authLoading } = useAuth()
  const router = useRouter()
  const queryClient = useQueryClient()

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      router.replace('/login')
    }
  }, [authLoading, isAuthenticated, router])

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.MySettings],
    queryFn: async () => await doQueryGet<MySettings>('/api/Me/Settings'),
    enabled: isAuthenticated
  })

  const save = useMutation({
    mutationFn: async (setting: MyChannelSetting) => {
      const res = await doPut('/api/Me/Settings', { body: setting })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save that setting')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Your settings were updated', 3000, <IconCheck />)
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.MySettings] })
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Title order={2}>Your settings</Title>

      <QueryState
        isLoading={authLoading || isLoading}
        isError={isError}
        isEmpty={data?.channels.length === 0}
        emptyMessage="You don't have any Discord stats yet, so there's nothing to configure."
      >
        <Stack gap="md">
          {data?.channels.map(setting => (
            <Card key={setting.broadcasterChannelName} withBorder padding="lg" radius="md">
              <Stack gap="md">
                <Text fw={600} tt="capitalize">{setting.broadcasterChannelName}</Text>

                <Switch
                  label="Discord level up messages"
                  description="Whether the bot announces when you level up in this channel's Discord"
                  checked={setting.discordLevelUpNotifsEnabled}
                  onChange={event => save.mutate({
                    broadcasterChannelName: setting.broadcasterChannelName,
                    discordLevelUpNotifsEnabled: event.currentTarget.checked
                  })}
                />
              </Stack>
            </Card>
          ))}
        </Stack>
      </QueryState>
    </Stack>
  )
}
