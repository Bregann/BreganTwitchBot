'use client'

import { Alert, Button, Card, Center, Stack, Text, Title } from '@mantine/core'
import { IconBrandTwitch } from '@tabler/icons-react'
import { useSearchParams } from 'next/navigation'
import { Suspense } from 'react'

const errorMessages: Record<string, string> = {
  declined: 'You declined the Twitch login, so nothing happened.',
  invalid: 'That login attempt looked wrong, so it was rejected. Please try again.',
  failed: 'Something went wrong signing you in. Please try again.'
}

function LoginContent() {
  const searchParams = useSearchParams()
  const error = searchParams.get('error')
  const errorMessage = error !== null ? errorMessages[error] ?? errorMessages.failed : null

  return (
    <Center mih="100vh" p="md">
      <Card withBorder padding="xl" radius="md" w={420}>
        <Stack gap="lg">
          <Stack gap={4}>
            <Title order={2}>Sign in</Title>
            <Text c="dimmed" size="sm">
              Sign in with Twitch to see your points, watchtime and rank in every channel running the bot.
            </Text>
          </Stack>

          {errorMessage !== null && (
            <Alert color="red" variant="light">{errorMessage}</Alert>
          )}

          <Button
            component="a"
            href="/api/Auth/Login"
            leftSection={<IconBrandTwitch size={18} />}
            size="md"
            fullWidth
          >
            Continue with Twitch
          </Button>

          <Text c="dimmed" size="xs" ta="center">
            We only ask Twitch who you are. The bot never gets access to your account.
          </Text>
        </Stack>
      </Card>
    </Center>
  )
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginContent />
    </Suspense>
  )
}
