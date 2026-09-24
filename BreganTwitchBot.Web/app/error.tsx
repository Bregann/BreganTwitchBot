'use client'

import { Button, Center, Group, Stack, Text, Title } from '@mantine/core'
import Link from 'next/link'
import { useEffect } from 'react'

export default function Error({
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
    <Center mih="60vh" p="md">
      <Stack gap="md" align="center" ta="center">
        <Title order={1}>Something went wrong</Title>
        <Text c="dimmed" maw={420}>
          That page failed to load. Trying again often fixes it.
        </Text>
        <Group>
          <Button onClick={reset}>Try again</Button>
          <Button component={Link} href="/" variant="default">Go home</Button>
        </Group>
      </Stack>
    </Center>
  )
}
