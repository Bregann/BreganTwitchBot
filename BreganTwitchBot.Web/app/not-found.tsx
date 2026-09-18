import { Button, Center, Stack, Text, Title } from '@mantine/core'
import Link from 'next/link'

export default function NotFound() {
  return (
    <Center mih="60vh" p="md">
      <Stack gap="md" align="center" ta="center">
        <Title order={1}>Not found</Title>
        <Text c="dimmed" maw={420}>
          That page doesn&apos;t exist. If you were looking for a channel, check the spelling.
        </Text>
        <Button component={Link} href="/">Go home</Button>
      </Stack>
    </Center>
  )
}
