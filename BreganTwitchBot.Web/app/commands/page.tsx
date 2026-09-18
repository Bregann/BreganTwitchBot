'use client'

import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { PublicCommand } from '@/interfaces/api/public/PublicCommand'
import { Alert, Badge, Card, Container, Grid, Group, Loader, Center, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconSearch } from '@tabler/icons-react'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

export default function CommandsPage() {
  const [search, setSearch] = useState('')

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.PublicCommands],
    queryFn: async () => await doQueryGet<PublicCommand[]>('/api/Public/Commands')
  })

  const commands = (data ?? []).filter(command =>
    command.commandName.includes(search.toLowerCase()) ||
    command.aliases.some(alias => alias.includes(search.toLowerCase()))
  )

  return (
    <Container size="lg" py={60}>
      <Stack gap="xl">
        <Stack gap="xs">
          <Title order={1}>Commands</Title>
          <Text c="dimmed">
            Every built in command. Channels can add their own on top of these.
          </Text>
        </Stack>

        <TextInput
          placeholder="Search commands"
          leftSection={<IconSearch size={16} />}
          value={search}
          onChange={event => setSearch(event.currentTarget.value)}
          maw={360}
        />

        {isLoading && (
          <Center py="xl"><Loader /></Center>
        )}

        {isError && (
          <Alert color="red" variant="light">
            The command list could not be loaded. Please try again later.
          </Alert>
        )}

        {!isLoading && !isError && commands.length === 0 && (
          <Text c="dimmed">No commands match that search.</Text>
        )}

        <Grid>
          {commands.map(command => (
            <Grid.Col key={command.commandName} span={{ base: 12, sm: 6, md: 4 }}>
              <Card withBorder padding="md" radius="md" h="100%">
                <Stack gap="xs">
                  <Text fw={600} ff="monospace">!{command.commandName}</Text>

                  {command.aliases.length > 0 && (
                    <Group gap={4}>
                      {command.aliases.map(alias => (
                        <Badge key={alias} size="sm" variant="light">!{alias}</Badge>
                      ))}
                    </Group>
                  )}
                </Stack>
              </Card>
            </Grid.Col>
          ))}
        </Grid>
      </Stack>
    </Container>
  )
}
