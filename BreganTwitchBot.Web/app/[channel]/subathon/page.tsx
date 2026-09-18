'use client'

import QueryState from '@/components/channel/QueryState'
import SubathonCountdown from '@/components/channel/SubathonCountdown'
import { doQueryGet } from '@/helpers/apiClient'
import { QueryKeys } from '@/helpers/QueryKeys'
import { SubathonLeaderboard, SubathonStatus } from '@/interfaces/api/channel/ChannelTypes'
import { Alert, Card, Grid, Group, Stack, Table, Text, Title } from '@mantine/core'
import { useQuery } from '@tanstack/react-query'
import { useParams } from 'next/navigation'

export default function ChannelSubathonPage() {
  const { channel } = useParams<{ channel: string }>()

  const { data, isLoading, isError } = useQuery({
    queryKey: [QueryKeys.SubathonStatus, channel],
    queryFn: async () => await doQueryGet<SubathonStatus>(`/api/Subathon/${channel}/status`),
    // resync the countdown periodically in case time has been added
    refetchInterval: 30000
  })

  const { data: leaderboard } = useQuery({
    queryKey: [QueryKeys.SubathonLeaderboard, channel],
    queryFn: async () => await doQueryGet<SubathonLeaderboard>(`/api/Subathon/${channel}/leaderboard`),
    enabled: data?.active === true
  })

  return (
    <QueryState isLoading={isLoading} isError={isError}>
      <Stack gap="xl">
        {data?.active !== true && (
          <Alert variant="light">There isn&apos;t a subathon running in this channel at the moment.</Alert>
        )}

        {data?.active === true && (
          <>
            <Card withBorder padding="xl" radius="md">
              <Stack gap="md" align="center">
                <SubathonCountdown secondsLeft={data.secondsLeft} />
                <Group gap="xs">
                  <Text c="dimmed" size="sm">Total time added:</Text>
                  <Text size="sm" fw={600}>{data.totalTimeAdded}</Text>
                </Group>
              </Stack>
            </Card>

            <Grid>
              <Grid.Col span={{ base: 12, md: 6 }}>
                <ContributorTable title="Top bits" rows={leaderboard?.topBitsDonators ?? []} unit="bits" />
              </Grid.Col>
              <Grid.Col span={{ base: 12, md: 6 }}>
                <ContributorTable title="Top gifted subs" rows={leaderboard?.topSubGifters ?? []} unit="subs" />
              </Grid.Col>
            </Grid>
          </>
        )}
      </Stack>
    </QueryState>
  )
}

function ContributorTable({
  title,
  rows,
  unit
}: {
  title: string
  rows: { position: number, username: string, amount: number }[]
  unit: string
}) {
  return (
    <Card withBorder padding="md" radius="md" h="100%">
      <Stack gap="sm">
        <Title order={4}>{title}</Title>

        {rows.length === 0 && <Text c="dimmed" size="sm">Nobody yet.</Text>}

        {rows.length > 0 && (
          <Table>
            <Table.Tbody>
              {rows.map(row => (
                <Table.Tr key={`${row.position}-${row.username}`}>
                  <Table.Td w={40}>{row.position}</Table.Td>
                  <Table.Td>{row.username}</Table.Td>
                  <Table.Td ta="right">{row.amount.toLocaleString()} {unit}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Stack>
    </Card>
  )
}
