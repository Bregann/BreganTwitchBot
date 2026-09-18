'use client'

import { Alert, Button, Group, Stack, Text } from '@mantine/core'
import Link from 'next/link'
import { useEffect } from 'react'

export default function ChannelError({
  error,
  reset
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    console.error(error)
  }, [error])

  return (
    <Stack gap="md">
      <Alert color="red" variant="light" title="That didn't load">
        <Text size="sm">
          This channel&apos;s data could not be loaded. It might not exist, or the bot may be
          having a moment.
        </Text>
      </Alert>

      <Group>
        <Button onClick={reset} size="sm">Try again</Button>
        <Button component={Link} href="/" variant="default" size="sm">Go home</Button>
      </Group>
    </Stack>
  )
}
