import { Card, Stack, Text } from '@mantine/core'

export default function StatCard({ label, value }: { label: string, value: string | number }) {
  return (
    <Card withBorder padding="md" radius="md">
      <Stack gap={2}>
        <Text size="xs" c="dimmed" tt="uppercase" fw={600}>{label}</Text>
        <Text size="xl" fw={700}>{typeof value === 'number' ? value.toLocaleString() : value}</Text>
      </Stack>
    </Card>
  )
}
