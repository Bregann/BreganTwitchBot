'use client'

import QueryState from '@/components/channel/QueryState'
import { doPut, doQueryGet } from '@/helpers/apiClient'
import notificationHelper from '@/helpers/notificationHelper'
import { QueryKeys } from '@/helpers/QueryKeys'
import { DiscordConfig } from '@/interfaces/api/admin/AdminTypes'
import { Button, Card, Group, NumberInput, Stack, Switch, Title } from '@mantine/core'
import { IconCheck, IconX } from '@tabler/icons-react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams } from 'next/navigation'
import { useEffect, useState } from 'react'

const idFields: { key: keyof DiscordConfig, label: string, description: string }[] = [
  { key: 'discordGuildId', label: 'Guild id', description: 'The server the bot works in' },
  { key: 'discordEventChannelId', label: 'Event channel', description: 'Stream summaries and follower updates' },
  { key: 'discordStreamAnnouncementChannelId', label: 'Stream announcements', description: 'Where going live is announced' },
  { key: 'discordUserCommandsChannelId', label: 'Commands channel', description: 'Where viewers use slash commands' },
  { key: 'discordUserRankUpAnnouncementChannelId', label: 'Rank up announcements', description: 'Where rank ups are posted' },
  { key: 'discordGiveawayChannelId', label: 'Giveaway channel', description: 'Where giveaways can be started' },
  { key: 'discordGeneralChannelId', label: 'General channel', description: 'Used for general messages' },
  { key: 'discordWelcomeMessageChannelId', label: 'Welcome channel', description: 'Where new members are greeted' },
  { key: 'discordModeratorRoleId', label: 'Moderator role', description: 'Role treated as a mod on the website' },
]

export default function AdminDiscordPage() {
  const { channel } = useParams<{ channel: string }>()
  const queryClient = useQueryClient()

  const [config, setConfig] = useState<DiscordConfig | null>(null)

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.DiscordConfig, channel],
    queryFn: async () => await doQueryGet<DiscordConfig>(`/api/Channels/${channel}/Admin/Discord`)
  })

  useEffect(() => {
    if (data !== undefined) {
      setConfig(data)
    }
  }, [data])

  const save = useMutation({
    mutationFn: async () => {
      const res = await doPut(`/api/Channels/${channel}/Admin/Discord`, { body: config })

      if (!res.ok) {
        throw new Error(res.statusMessage ?? 'Could not save the Discord settings')
      }
    },
    onSuccess: () => {
      notificationHelper.showSuccessNotification('Saved', 'Discord settings updated', 4000, <IconCheck />)
      void queryClient.invalidateQueries({ queryKey: [QueryKeys.DiscordConfig, channel] })
    },
    onError: (error: Error) => {
      notificationHelper.showErrorNotification('Could not save', error.message, 6000, <IconX />)
    }
  })

  return (
    <Stack gap="lg">
      <Title order={3}>Discord</Title>

      <QueryState isLoading={isLoading} isError={isError} errorMessage="You do not have permission to change the Discord settings.">
        <Card withBorder padding="lg" radius="md">
          <Stack gap="md">
            <Switch
              label="Discord enabled"
              description="Turn the Discord side of the bot on for this channel"
              checked={config?.discordEnabled ?? false}
              onChange={event => setConfig(current => current === null ? current : { ...current, discordEnabled: event.currentTarget.checked })}
            />

            {idFields.map(field => (
              <NumberInput
                key={field.key}
                label={field.label}
                description={field.description}
                value={(config?.[field.key] as number | null) ?? ''}
                onChange={value => setConfig(current => current === null
                  ? current
                  : { ...current, [field.key]: value === '' ? null : Number(value) })}
                hideControls
              />
            ))}

            <Group justify="flex-end">
              <Button onClick={() => save.mutate()} loading={save.isPending}>Save</Button>
            </Group>
          </Stack>
        </Card>
      </QueryState>
    </Stack>
  )
}
