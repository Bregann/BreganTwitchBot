import ChannelNav from '@/components/channel/ChannelNav'
import { Container } from '@mantine/core'
import type { Metadata } from 'next'

export async function generateMetadata({ params }: { params: Promise<{ channel: string }> }): Promise<Metadata> {
  const { channel } = await params

  return {
    title: channel
  }
}

export default async function ChannelLayout({
  children,
  params
}: {
  children: React.ReactNode
  params: Promise<{ channel: string }>
}) {
  const { channel } = await params

  return (
    <Container size="lg" py="lg">
      <ChannelNav channel={channel} />
      {children}
    </Container>
  )
}
