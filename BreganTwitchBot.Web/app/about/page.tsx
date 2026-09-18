import { Anchor, Container, Stack, Text, Title } from '@mantine/core'
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'About'
}

export default function AboutPage() {
  return (
    <Container size="md" py={60}>
      <Stack gap="lg">
        <Title order={1}>About</Title>

        <Text>
          BreganTwitchBot is a Twitch and Discord bot that tracks points, watchtime, ranks,
          subathons and more for a channel. It runs as a single bot across every channel it
          serves, with all configuration held per channel.
        </Text>

        <Text>
          The bot reads chat, awards points and watchtime, manages Discord roles and keeps
          a record of every stream. Viewers can sign in here with Twitch to see their own
          numbers, and broadcasters can configure everything without touching a database.
        </Text>

        <Text>
          It is open source.{' '}
          <Anchor href="https://github.com/Bregann/BreganTwitchBot" target="_blank">
            The code is on GitHub
          </Anchor>
          .
        </Text>
      </Stack>
    </Container>
  )
}
