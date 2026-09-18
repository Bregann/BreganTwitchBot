import { Container, List, Stack, Text, ThemeIcon, Title } from '@mantine/core'
import { IconCheck } from '@tabler/icons-react'
import type { Metadata } from 'next'

export const metadata: Metadata = {
  title: 'Features'
}

const sections = [
  {
    title: 'Points and currency',
    items: [
      'Channel currency with a name you choose',
      'Daily, weekly, monthly and yearly claims, each with their own streak',
      'Points for watching, subscribing and cheering',
      'Gambling and a slot machine, with per channel limits',
    ]
  },
  {
    title: 'Watchtime and ranks',
    items: [
      'Minutes tracked per stream, week, month, year and all time',
      'Ranks you define, awarded automatically as people watch',
      'Matching Discord roles applied on rank up',
    ]
  },
  {
    title: 'Subathons',
    items: [
      'Bits and subs add time on a rate scale you control',
      'Rates taper as the total grows, so a long subathon stays finite',
      'Live timer and a per user contributor leaderboard',
      'Start, stop and add time from chat or the web',
    ]
  },
  {
    title: 'Discord',
    items: [
      'Levelling with xp, prestige and level up announcements',
      'Giveaways weighted by watchtime and activity',
      'Self assign role buttons',
      'Monthly bits and gifted sub leaderboard roles',
      'Stream announcements and end of stream stat summaries',
    ]
  },
  {
    title: 'Moderation',
    items: [
      'Word blacklist with warn, timeout and permanent ban tiers',
      'Per channel configuration for every limit',
    ]
  },
]

export default function FeaturesPage() {
  return (
    <Container size="md" py={60}>
      <Stack gap="xl">
        <Stack gap="xs">
          <Title order={1}>Features</Title>
          <Text c="dimmed">Everything the bot does, per channel and configurable.</Text>
        </Stack>

        {sections.map(section => (
          <Stack key={section.title} gap="sm">
            <Title order={3}>{section.title}</Title>
            <List
              spacing="xs"
              icon={
                <ThemeIcon size={20} radius="xl" variant="light">
                  <IconCheck size={12} />
                </ThemeIcon>
              }
            >
              {section.items.map(item => (
                <List.Item key={item}>{item}</List.Item>
              ))}
            </List>
          </Stack>
        ))}
      </Stack>
    </Container>
  )
}
