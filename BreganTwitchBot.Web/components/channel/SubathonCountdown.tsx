'use client'

import { Stack, Text, Title } from '@mantine/core'
import { useEffect, useState } from 'react'

/**
 * Counts down from the seconds the api reported.
 *
 * The api gives a number of seconds at the moment it answered, so this ticks it
 * down locally rather than asking the server every second. The query that feeds
 * it refetches periodically, which resyncs the number if time has been added.
 */
export default function SubathonCountdown({ secondsLeft }: { secondsLeft: number }) {
  const [remaining, setRemaining] = useState(secondsLeft)

  useEffect(() => {
    setRemaining(secondsLeft)
  }, [secondsLeft])

  useEffect(() => {
    const interval = setInterval(() => {
      setRemaining(previous => (previous > 0 ? previous - 1 : 0))
    }, 1000)

    return () => clearInterval(interval)
  }, [])

  if (remaining <= 0) {
    return (
      <Stack gap={0} align="center">
        <Title order={1}>Time is up</Title>
        <Text c="dimmed">The subathon has run out of time</Text>
      </Stack>
    )
  }

  const days = Math.floor(remaining / 86400)
  const hours = Math.floor((remaining % 86400) / 3600)
  const minutes = Math.floor((remaining % 3600) / 60)
  const seconds = remaining % 60

  const pad = (value: number): string => value.toString().padStart(2, '0')

  return (
    <Stack gap={0} align="center">
      <Title order={1} ff="monospace" size={56}>
        {days > 0 && `${days}d `}{pad(hours)}:{pad(minutes)}:{pad(seconds)}
      </Title>
      <Text c="dimmed">remaining</Text>
    </Stack>
  )
}
