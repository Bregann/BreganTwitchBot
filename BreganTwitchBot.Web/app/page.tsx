import { Button, Container, Group, Stack, Text, Title } from '@mantine/core'
import Link from 'next/link'

export default function HomePage() {
  return (
    <Container size="lg" py={80}>
      <Stack gap="lg" align="center" ta="center">
        <Title order={1} size={48}>BreganTwitchBot</Title>
        <Text size="xl" c="dimmed" maw={640}>
          Points, watchtime, subathons, giveaways and more for your Twitch channel and Discord server.
        </Text>
        <Group>
          <Button component={Link} href="/login" size="md">Login with Twitch</Button>
          <Button component={Link} href="/features" size="md" variant="default">See features</Button>
        </Group>
      </Stack>
    </Container>
  )
}
