import { Badge, Button, Card, Container, Grid, Group, Stack, Text, Title } from '@mantine/core'
import {
  IconBrandDiscord,
  IconBrandTwitch,
  IconClock,
  IconCoin,
  IconGift,
  IconTrophy
} from '@tabler/icons-react'
import Link from 'next/link'

const features = [
  { icon: IconCoin, title: 'Points and daily rewards', description: 'Viewers earn channel currency for watching, with daily, weekly, monthly and yearly claims and streaks.' },
  { icon: IconClock, title: 'Watchtime and ranks', description: 'Tracks how long everybody watches and promotes them through ranks you configure, with matching Discord roles.' },
  { icon: IconClock, title: 'Subathons', description: 'Bits and subs add time on a scale you control, with a live timer and a contributor leaderboard.' },
  { icon: IconTrophy, title: 'Leaderboards', description: 'Points, hours, streaks, gambling and more, in chat, in Discord and on the web.' },
  { icon: IconGift, title: 'Giveaways', description: 'Run Discord giveaways weighted by watchtime and activity, with entry requirements you set.' },
  { icon: IconBrandDiscord, title: 'Discord integration', description: 'Levelling, role management, stream announcements and end of stream summaries.' },
]

export default function HomePage() {
  return (
    <>
      <Container size="lg" py={80}>
        <Stack gap="xl" align="center" ta="center">
          <Badge size="lg" variant="light">Twitch and Discord</Badge>

          <Title order={1} size={52} maw={760}>
            Everything your channel needs, in one bot
          </Title>

          <Text size="xl" c="dimmed" maw={640}>
            Points, watchtime, ranks, subathons, giveaways and leaderboards, tracked across
            your stream and your Discord server.
          </Text>

          <Group>
            <Button component={Link} href="/login" size="md" leftSection={<IconBrandTwitch size={18} />}>
              Sign in with Twitch
            </Button>
            <Button component={Link} href="/features" size="md" variant="default">
              See what it does
            </Button>
          </Group>
        </Stack>
      </Container>

      <Container size="lg" pb={80}>
        <Grid>
          {features.map(feature => (
            <Grid.Col key={feature.title} span={{ base: 12, sm: 6, md: 4 }}>
              <Card withBorder padding="lg" radius="md" h="100%">
                <Stack gap="sm">
                  <feature.icon size={28} />
                  <Text fw={600}>{feature.title}</Text>
                  <Text size="sm" c="dimmed">{feature.description}</Text>
                </Stack>
              </Card>
            </Grid.Col>
          ))}
        </Grid>
      </Container>
    </>
  )
}
